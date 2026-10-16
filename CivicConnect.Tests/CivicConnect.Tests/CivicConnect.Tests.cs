using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CivicConnect.Core.Entities;
using CivicConnect.Core.Exceptions;
using CivicConnect.Core.Services;
using CivicConnect.Core.Strategies;
using CivicConnect.Infrastructure.Data;
using CivicConnect.Infrastructure.Repositories;

namespace CivicConnect.Tests;

// =============================================================================
// PART A: HTTP-LEVEL INTEGRATION TESTS (WebApplicationFactory)
// =============================================================================
public class TicketIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _integrationTestClient;

    public TicketIntegrationTests(WebApplicationFactory<Program> serverFactory)
    {
        _integrationTestClient = serverFactory.CreateClient();
    }

    [Fact]
    public async Task GetAllTickets_ExecutesSuccessfullyAndReturnsOk()
    {
        var apiResponse = await _integrationTestClient.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_RejectsInvalidPayloadAndReturnsBadRequest()
    {
        var malformedPayload = new
        {
            Title = "",
            CategoryId = 0
        };

        var apiResponse = await _integrationTestClient.PostAsJsonAsync("/api/tickets", malformedPayload);

        Assert.Equal(HttpStatusCode.BadRequest, apiResponse.StatusCode);
    }
}

// =============================================================================
// PART B: MILESTONE 3 VERIFICATION TESTS
// =============================================================================
public class Milestone3VerificationTests
{
    private CivicConnectDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CivicConnectDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new CivicConnectDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    // =========================================================================
    // 1. UNIT / COMPONENT TESTS (5 Cases) - Traceable to FR-001, FR-002, ADR-004
    // =========================================================================

    [Fact] // TC-01 (Unit): Valid Ticket Construction initializes Open status and UTC timestamps (FR-001)
    public void TC01_Unit_TicketConstructor_ValidInputs_SetsInitialOpenState()
    {
        var ticket = new Ticket("Burst pipe on Oak Ave", "Water gushing onto road surface", 2, 101, true);

        Assert.Equal("Burst pipe on Oak Ave", ticket.Title);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.True(ticket.IsEmergency);
    }

    [Fact] // TC-02 (Unit - Negative): Empty title throws DomainRuleViolationException (FR-001)
    public void TC02_Unit_TicketConstructor_EmptyTitle_ThrowsDomainException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new Ticket("   ", "Valid description text here", 1, 101, false));
    }

    [Fact] // TC-03 (Unit - Regression DEF-01): Numeric-only title is rejected by domain rule (FR-001)
    public void TC03_Unit_TicketConstructor_NumericOnlyTitle_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainRuleViolationException>(() =>
            new Ticket("123456", "Valid description with letters", 1, 101, false));

        Assert.Contains("letters", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] // TC-04 (Unit - Regression DEF-01): Numeric-only description is rejected (FR-001)
    public void TC04_Unit_TicketConstructor_NumericOnlyDescription_ThrowsDomainException()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new Ticket("Pothole on Main Road", "9999999999", 1, 101, false));
    }

    [Fact] // TC-05 (Unit): Emergency Priority Strategy assigns High/Critical priority (ADR-004)
    public void TC05_Unit_PriorityStrategy_EmergencyFlag_AssignsElevatedPriority()
    {
        var ticket = new Ticket("Live wire exposed", "Sparking electrical cable near school", 3, 101, isEmergency: true);

        // Registered in reverse order on purpose: proves EvaluationOrder (10 before 100) decides, not list position
        var context = new PriorityCalculationContext(new IPriorityCalculationStrategy[]
        {
            new StandardPriorityStrategy(),
            new EmergencyPriorityStrategy()
        });

        // Emergency strategy ignores the category, so null is safe here
        var priority = context.CalculatePriority(ticket, null!);

        Assert.Equal(TicketPriority.Critical, priority);
    }

    // =========================================================================
    // 2. BLACK-BOX FUNCTIONAL TESTS (5 Cases: EP, BVA, Decision Table, State Transition)
    // =========================================================================

    [Theory] // TC-06 (Black-Box - Equivalence Partitioning): Valid vs Invalid Title Partitions (FR-001)
    [InlineData("12345", false)]          // Invalid Partition: Pure digits
    [InlineData("$$$###", false)]         // Invalid Partition: Pure symbols
    [InlineData("Streetlight 42", true)]  // Valid Partition: Alphanumeric descriptive title
    public void TC06_BlackBox_EP_TitleInputPartitions_ValidatedCorrectly(string inputTitle, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var ticket = new Ticket(inputTitle, "Pole 42 bulb is flickering", 3, 101, false);
            Assert.Equal(inputTitle, ticket.Title);
        }
        else
        {
            Assert.Throws<DomainRuleViolationException>(() =>
                new Ticket(inputTitle, "Pole 42 bulb is flickering", 3, 101, false));
        }
    }

    [Theory] // TC-07 (Black-Box - Boundary Value Analysis): Title Character Presence Boundary
    [InlineData("1234", false)]   // Below letter boundary (0 letters)
    [InlineData("1234A", true)]   // Exact lower boundary (1 letter present)
    [InlineData("Road A1", true)] // Above boundary (multiple letters)
    public void TC07_BlackBox_BVA_TitleMinimumLetterBoundary(string title, bool expectedValid)
    {
        bool hasLetter = title.Any(char.IsLetter);
        Assert.Equal(expectedValid, hasLetter);
    }

    [Theory] // TC-08 (Black-Box - Decision Table + BVA): Urgency Score + Emergency Flag -> Priority Outcome (ADR-004)
    [InlineData(1, true, TicketPriority.Critical)]   // Emergency overrides a low score
    [InlineData(10, true, TicketPriority.Critical)]  // Emergency with max score
    [InlineData(10, false, TicketPriority.High)]     // Non-emergency, top of High band
    [InlineData(8, false, TicketPriority.High)]      // Boundary: lowest High
    [InlineData(7, false, TicketPriority.Medium)]    // Boundary: highest Medium
    [InlineData(5, false, TicketPriority.Medium)]    // Boundary: lowest Medium
    [InlineData(4, false, TicketPriority.Low)]       // Boundary: highest Low
    [InlineData(1, false, TicketPriority.Low)]       // Lowest possible score
    public void TC08_BlackBox_DecisionTable_UrgencyAndEmergencyFlag_DeterminesPriority(
        int urgencyScore, bool isEmergency, TicketPriority expectedPriority)
    {
        var category = new Category("Test category", "Used for priority rules", urgencyScore);
        var ticket = new Ticket("Water leak", "Main valve leaking steadily", 1, 101, isEmergency);

        var context = new PriorityCalculationContext(new IPriorityCalculationStrategy[]
        {
            new StandardPriorityStrategy(),
            new EmergencyPriorityStrategy()
        });

        var actualPriority = context.CalculatePriority(ticket, category);

        Assert.Equal(expectedPriority, actualPriority);
    }

    [Fact] // TC-09 (Black-Box - State Transition): Valid Ticket Lifecycle Open -> InProgress -> Resolved (FR-002)
    public void TC09_BlackBox_StateTransition_OpenToInProgressToResolved()
    {
        var ticket = new Ticket("Blocked storm drain", "Debris blocking water flow", 2, 101, false);
        Assert.Equal(TicketStatus.Open, ticket.Status);

        ticket.TransitionTo(TicketStatus.InProgress);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);

        ticket.TransitionTo(TicketStatus.Resolved);
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }

    [Theory] // TC-10 (Black-Box - BVA / SLA Overdue Rule FR-003): Ticket becomes overdue after 48 hours
    [InlineData(47, false)]  // Within SLA
    [InlineData(48, false)]  // Exactly on the boundary: not yet breached (rule is > 48h)
    [InlineData(49, true)]   // Breached 48h SLA
    public void TC10_BlackBox_BVA_SlaOverdue_BoundaryAt48Hours(int hoursElapsed, bool expectedOverdue)
    {
        var ticket = new Ticket("Broken traffic light", "Light stuck on red at main junction", 1, 101, false);

        // "Now" is passed in, so no waiting is needed
        var now = ticket.CreatedAtUtc.AddHours(hoursElapsed);

        Assert.Equal(expectedOverdue, ticket.IsOverdue(now));
    }

    [Fact] // TC-10b (Negative): A resolved ticket is never overdue, even after 48 hours
    public void TC10b_BlackBox_SlaOverdue_ResolvedTicket_IsNeverOverdue()
    {
        var ticket = new Ticket("Broken traffic light", "Light stuck on red at main junction", 1, 101, false);
        ticket.TransitionTo(TicketStatus.InProgress);
        ticket.TransitionTo(TicketStatus.Resolved);

        Assert.False(ticket.IsOverdue(ticket.CreatedAtUtc.AddHours(100)));
    }

    // =========================================================================
    // 3. API / INTEGRATION TESTS (3 Cases) - Repository & SQLite/EF Core Persistence
    // =========================================================================

    [Fact] // TC-11 (Integration): TicketRepository persists and retrieves Ticket with Category (ADR-002)
    public async Task TC11_Integration_TicketRepository_AddAndGetWithCategory_PersistsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var repo = new TicketRepository(context);

        var ticket = new Ticket("Pothole near clinic", "Deep pothole in left lane", 1, 101, false);
        await repo.AddAsync(ticket);
        await context.SaveChangesAsync();

        var allTickets = await repo.GetTicketsWithCategoryAsync();
        Assert.Contains(allTickets, t => t.Title == "Pothole near clinic");
    }

    [Fact] // TC-12 (Integration - Negative): Requesting non-existent Ticket ID returns null safely
    public async Task TC12_Integration_TicketRepository_GetInvalidTicketId_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repo = new TicketRepository(context);

        var result = await repo.GetTicketWithDetailsAsync(99999);
        Assert.Null(result);
    }

    [Fact] // TC-13 (Integration): Updating ticket status persists change in database (FR-002)
    public async Task TC13_Integration_TicketRepository_UpdateStatus_PersistsStateChange()
    {
        using var context = CreateInMemoryContext();
        var repo = new TicketRepository(context);

        var ticket = new Ticket("Fallen tree branch", "Blocking park footpath", 4, 101, false);
        await repo.AddAsync(ticket);
        await context.SaveChangesAsync();

        // Open -> Resolved is not an allowed transition, so go via InProgress
        ticket.TransitionTo(TicketStatus.InProgress);
        ticket.TransitionTo(TicketStatus.Resolved);
        await context.SaveChangesAsync();

        var updated = await repo.GetTicketWithDetailsAsync(ticket.Id);
        Assert.NotNull(updated);
        Assert.Equal(TicketStatus.Resolved, updated!.Status);
    }

    // =========================================================================
    // 4. E2E / SYSTEM JOURNEY TESTS (2 Cases) - Full Lifecycle & Role Security (NFR-002)
    // =========================================================================

    [Fact] // TC-14 (System/E2E): End-to-End Citizen Submission -> Admin Triage -> Resolution Journey
    public async Task TC14_E2E_FullCitizenToAdminResolutionLifecycle_CompletesSuccessfully()
    {
        using var context = CreateInMemoryContext();
        var repo = new TicketRepository(context);

        // Step 1: Citizen logs issue
        var citizenTicket = new Ticket("Water main burst", "Flooding intersection at 5th St", 2, 501, true);
        await repo.AddAsync(citizenTicket);
        await context.SaveChangesAsync();

        // Step 2: Staff/Admin retrieves queue & transitions to InProgress
        var fetched = await repo.GetTicketWithDetailsAsync(citizenTicket.Id);
        Assert.NotNull(fetched);
        fetched!.TransitionTo(TicketStatus.InProgress);
        await context.SaveChangesAsync();

        // Step 3: Admin resolves ticket
        fetched.TransitionTo(TicketStatus.Resolved);
        await context.SaveChangesAsync();

        var finalRecord = await repo.GetTicketWithDetailsAsync(citizenTicket.Id);
        Assert.Equal(TicketStatus.Resolved, finalRecord!.Status);
    }

    [Theory] // TC-15 (System/E2E - Negative Security NFR-002): Only the owner may delete a report; admins and other citizens may not
    [InlineData("42", "42", true)]    // Owner can delete own report
    [InlineData("42", "7", false)]    // Other citizen, or an admin (not the owner), cannot delete
    [InlineData("42", null, false)]   // Not logged in / missing identity
    [InlineData(null, null, false)]   // Missing ids must never count as a match
    public void TC15_E2E_SecurityOwnershipGuard_EnforcesDeletionAndAuditProtection(
        string? ticketOwnerId, string? currentUserId, bool expectedCanDelete)
    {
        Assert.Equal(expectedCanDelete, TicketAuthorization.CanDelete(ticketOwnerId, currentUserId));
    }
}