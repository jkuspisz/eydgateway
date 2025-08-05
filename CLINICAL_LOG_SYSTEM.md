# Clinical Log System Documentation

## Overview
The Clinical Log system allows EYD (Early Years Dentist) users to track their monthly clinical procedures and activities. This system provides a structured way to record numerical entries for 47 specific clinical procedures organized by competency groups, with monthly tracking and completion workflow.

## Features

###3. System sets IsCompleted = true and CompletedAt timestamp
4. Log becomes read-only (edit option removed)

### Viewing Total Numbers Analytics
1. From Index view, clicks "View Total Numbers"
2. System displays cumulative totals dashboard
3. Summary cards show overall progress statistics
4. Detailed table shows total numbers for all 47 procedures
5. Color-coded badges provide visual progress indicatorsore Functionality
- **Monthly Tracking**: EYDs can create logs for specific months and years
- **47 Clinical Procedures**: Comprehensive coverage of all required dental procedures
- **Competency Organization**: Procedures grouped into 8 major clinical competency areas
- **Completion Workflow**: Ability to mark monthly logs as complete
- **User-Scoped**: Each EYD can only see and manage their own clinical logs
- **Analytics Dashboard**: Total Numbers view showing cumulative totals across all months

### User Interface
- **Table Layout**: Concise 3-column design (Type, Procedure, Number)
- **Color-Coded Categories**: Visual organization with Bootstrap table colors
- **Responsive Design**: Works across different screen sizes
- **Form Validation**: Required fields and numerical constraints
- **Analytics View**: Total Numbers page with cumulative totals and summary statistics

## Database Schema

### ClinicalLog Model
```csharp
public class ClinicalLog
{
    public int Id { get; set; }
    public string EYDUserId { get; set; }
    public string Month { get; set; }
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    
    // 47 Clinical Procedure Properties (all int)
    // General (5)
    public int DNANumbers { get; set; }
    public int NumberOfUnbookedClinicalHours { get; set; }
    public int Holidays { get; set; }
    public int SickDays { get; set; }
    public int UDAsEvidencedOnPracticeSoftware { get; set; }
    
    // Clinical Assessment (6)
    public int AdultExaminations { get; set; }
    public int PaediatricExaminations { get; set; }
    public int AdultRadiographs { get; set; }
    public int PaediatricRadiographs { get; set; }
    public int PatientsWithComplexMedicalHistories { get; set; }
    public int SixPointPeriodontalChart { get; set; }
    
    // Oral Health Promotion (2)
    public int DietAnalysis { get; set; }
    public int FluorideVarnish { get; set; }
    
    // Medical and dental emergencies (3)
    public int ManagementOfMedicalEmergencyIncident { get; set; }
    public int DentalTrauma { get; set; }
    public int PulpExtripation { get; set; }
    
    // Prescribing and therapeutics (5)
    public int PrescribingAntimicrobials { get; set; }
    public int IVSedation { get; set; }
    public int InhalationalSedation { get; set; }
    public int GeneralAnaesthesiaPlanningAndConsent { get; set; }
    public int GeneralAnaesthesiaTreatmentUndertaken { get; set; }
    
    // Periodontal Disease (1)
    public int NonSurgicalTherapy { get; set; }
    
    // Removal of teeth (3)
    public int ExtractionOfPermanentTeeth { get; set; }
    public int ComplexExtractionInvolvingSectioning { get; set; }
    public int Suturing { get; set; }
    
    // Management of developing dentition (3)
    public int SSCrownsOnDeciduousTeeth { get; set; }
    public int ExtractionOfDeciduousTeeth { get; set; }
    public int OrthodonticAssessment { get; set; }
    
    // Restoration of teeth (13)
    public int RubberDamPlacement { get; set; }
    public int AmalgamRestorations { get; set; }
    public int AnteriorCompositeRestorations { get; set; }
    public int PosteriorCompositeRestorations { get; set; }
    public int GIC { get; set; }
    public int RCTIncisorCanine { get; set; }
    public int RCTPremolar { get; set; }
    public int RCTMolar { get; set; }
    public int CrownsConventional { get; set; }
    public int Onlays { get; set; }
    public int Posts { get; set; }
    public int BridgeResinRetained { get; set; }
    public int BridgeConventional { get; set; }
    
    // Replacement of teeth (3)
    public int AcrylicCompleteDentures { get; set; }
    public int AcrylicPartialDentures { get; set; }
    public int CobaltChromePartialDentures { get; set; }
    
    // Navigation Properties
    public ApplicationUser EYDUser { get; set; }
}
```

## Architecture

### File Structure
```
Controllers/
├── ClinicalLogController.cs       # Main CRUD operations
Models/
├── ClinicalLog.cs                 # Entity model
ViewModels/
├── ClinicalLogViewModels.cs       # DTOs for views
Views/ClinicalLog/
├── Index.cshtml                   # List view with search/filter
├── Create.cshtml                  # Table-based creation form
├── Edit.cshtml                    # Edit existing log
├── Details.cshtml                 # Read-only detailed view
├── Delete.cshtml                  # Confirmation delete view
└── TotalNumbers.cshtml            # Analytics dashboard with cumulative totals
Migrations/
└── AddClinicalLog.cs              # Database migration
```

### Controller Actions

#### ClinicalLogController
- **Index()**: List all clinical logs for current EYD user
- **Create()**: Display creation form
- **Create(CreateClinicalLogViewModel)**: Process form submission
- **Edit(int id)**: Display edit form for existing log
- **Edit(EditClinicalLogViewModel)**: Process edit submission
- **Details(int id)**: Show read-only detailed view
- **Delete(int id)**: Show delete confirmation
- **DeleteConfirmed(int id)**: Process deletion
- **Complete(int id)**: Mark log as completed
- **TotalNumbers()**: Display analytics dashboard with cumulative totals

### View Models

#### CreateClinicalLogViewModel
- Contains all 47 procedure properties
- EYDUserId (hidden)
- Month and Year selection
- Display attributes for proper labeling

#### EditClinicalLogViewModel
- Inherits from CreateClinicalLogViewModel
- Adds Id property
- Month/Year are read-only in edit mode

#### ClinicalLogTotalsViewModel
- Contains all 47 procedure totals (cumulative across all months)
- Summary statistics: TotalLogs, CompletedLogs
- Used for analytics and reporting in Total Numbers view

## User Interface Design

### Table Layout Structure
```html
<table class="table table-striped table-hover">
    <thead class="table-dark">
        <tr>
            <th style="width: 20%;">Type</th>
            <th style="width: 60%;">Procedure</th>
            <th style="width: 20%;">Number</th>
        </tr>
    </thead>
    <tbody>
        <!-- Color-coded category headers -->
        <tr class="table-secondary">
            <td colspan="3"><strong><i class="fas fa-cog"></i> General</strong></td>
        </tr>
        <!-- Individual procedure rows -->
        <tr>
            <td>General</td>
            <td>DNA numbers</td>
            <td><input asp-for="DNANumbers" type="number" class="form-control form-control-sm" min="0" /></td>
        </tr>
        <!-- Repeat for all 47 procedures... -->
    </tbody>
</table>
```

### Color Coding System
- **General**: `table-secondary` (Grey)
- **Clinical Assessment**: `table-info` (Light Blue)
- **Oral Health Promotion**: `table-success` (Green)
- **Medical/Dental Emergencies**: `table-danger` (Red)
- **Prescribing/Therapeutics**: `table-warning` (Yellow)
- **Periodontal Disease**: `table-dark text-white` (Dark)
- **Removal of teeth**: `table-secondary` (Grey)
- **Management of developing dentition**: `table-info` (Light Blue)
- **Restoration of teeth**: `table-primary` (Primary Blue)
- **Replacement of teeth**: `table-success` (Green)

## Total Numbers Analytics System

### Overview
The Total Numbers feature provides comprehensive analytics for EYDs to track their cumulative progress across all monthly clinical logs. This dashboard offers valuable insights into procedure completion patterns and overall training progress.

### Features

#### Summary Statistics
- **Total Months Logged**: Count of all clinical logs created
- **Completed Months**: Number of logs marked as complete
- **In Progress**: Number of logs still being edited
- **Completion Rate**: Visual progress indicator

#### Cumulative Totals
- **All 47 Procedures**: Shows sum of all entries across all months
- **Color-Coded Badges**: Visual indicators for different total ranges
  - **0 totals**: `badge bg-secondary` (Grey)
  - **1-5 totals**: `badge bg-info` (Light Blue)
  - **6-15 totals**: `badge bg-primary` (Primary Blue)
  - **16+ totals**: `badge bg-success` (Green)

### Implementation Details

#### Controller Logic
```csharp
public async Task<IActionResult> TotalNumbers()
{
    var userId = _userManager.GetUserId(User);
    var userLogs = _context.ClinicalLogs
        .Where(cl => cl.EYDUserId == userId)
        .ToList();

    var viewModel = new ClinicalLogTotalsViewModel
    {
        TotalLogs = userLogs.Count,
        CompletedLogs = userLogs.Count(cl => cl.IsCompleted),
        
        // Calculate cumulative totals for all 47 procedures
        DNANumbers = userLogs.Sum(cl => cl.DNANumbers),
        NumberOfUnbookedClinicalHours = userLogs.Sum(cl => cl.NumberOfUnbookedClinicalHours),
        // ... (all 47 procedures)
    };

    return View(viewModel);
}
```

#### View Structure
```html
<!-- Summary Cards -->
<div class="row mb-4">
    <div class="col-md-4">
        <div class="card bg-primary text-white">
            <div class="card-body text-center">
                <h3>@Model.TotalLogs</h3>
                <p>Total Months Logged</p>
            </div>
        </div>
    </div>
    <!-- Additional summary cards... -->
</div>

<!-- Cumulative Totals Table -->
<div class="table-responsive">
    <table class="table table-striped table-hover">
        <thead class="table-dark">
            <tr>
                <th>Type</th>
                <th>Procedure</th>
                <th>Total Numbers</th>
            </tr>
        </thead>
        <tbody>
            <!-- Color-coded category headers and totals -->
        </tbody>
    </table>
</div>
```

### Navigation
Accessible from the Clinical Log Index page via:
```html
<a class="btn btn-info ms-2" asp-action="TotalNumbers">
    <i class="fas fa-chart-bar"></i> View Total Numbers
</a>
```

### User Experience Benefits
1. **Progress Tracking**: EYDs can monitor their cumulative procedure experience
2. **Competency Assessment**: Identify areas requiring more focus
3. **Training Planning**: Make informed decisions about future clinical opportunities
4. **Portfolio Development**: Support evidence gathering for assessments
5. **Motivation**: Visual progress indicators encourage continued engagement

## Navigation Integration

### EYD Dashboard Integration
The Clinical Log is accessible through the EYD Dashboard portfolio sections with dynamic count display:

```csharp
// In EYDController.cs
new PortfolioSectionGroup
{
    GroupName = "Clinical Logs & Activities",
    Sections = new List<PortfolioSection>
    {
        new PortfolioSection
        {
            Title = "Monthly Clinical Log",
            Description = "Track monthly clinical procedures and activities",
            Controller = "ClinicalLog",
            Action = "Index",
            IconClass = "fas fa-clipboard-list",
            TotalCount = clinicalLogTotal,
            CompletedCount = clinicalLogComplete
        }
    }
}
```

### Count Display Features
- **Total Count**: Shows number of clinical logs created
- **Completed Count**: Shows number of logs marked as complete  
- **Format**: Displays as "X / Y items" below the section title
- **Real-time Updates**: Counts update automatically when logs are created or completed

## Database Configuration

### Entity Framework Configuration
```csharp
// In ApplicationDbContext.cs
public DbSet<ClinicalLog> ClinicalLogs { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<ClinicalLog>()
        .HasOne(cl => cl.EYDUser)
        .WithMany()
        .HasForeignKey(cl => cl.EYDUserId)
        .OnDelete(DeleteBehavior.Cascade);
        
    modelBuilder.Entity<ClinicalLog>()
        .HasIndex(cl => new { cl.EYDUserId, cl.Month, cl.Year })
        .IsUnique();
}
```

### Migration
```bash
dotnet ef migrations add AddClinicalLog
dotnet ef database update
```

## Security Features

### Authorization
- Users can only access their own clinical logs
- Controller actions verify user ownership
- Hidden EYDUserId field prevents tampering

### Validation
- Required Month and Year fields
- Numerical inputs with minimum value of 0
- Server-side model validation
- Duplicate prevention (one log per month/year per user)

## Usage Workflow

### Creating a New Clinical Log
1. EYD navigates to Dashboard → "Clinical Logs & Activities" → "Monthly Clinical Log"
2. Clicks "Create New Clinical Log"
3. Selects Month and Year
4. Fills in numerical values for relevant procedures
5. Clicks "Save Clinical Log"
6. System creates log with CreatedAt timestamp

### Editing an Existing Log
1. From Index view, clicks "Edit" on desired log
2. Month/Year fields are read-only
3. Modifies procedure numbers as needed
4. Saves changes

### Completing a Log
1. From Index or Details view, clicks "Complete"
2. System sets IsCompleted = true and CompletedAt timestamp
3. Log becomes read-only (edit option removed)

## Technical Implementation Notes

### Key Features
- **PostgreSQL Compatibility**: UTC DateTime handling for timezone consistency
- **Responsive Design**: Bootstrap table classes for mobile compatibility
- **Form Validation**: Client-side and server-side validation
- **User Experience**: Clear visual organization and intuitive navigation

### Performance Considerations
- Indexed on (EYDUserId, Month, Year) for fast lookups
- Lazy loading for navigation properties
- Efficient pagination for large datasets

### Error Handling
- Duplicate month/year prevention
- Access control validation
- Input validation and sanitization
- Graceful error messages

## Testing

### Test Scenarios
1. **Create Log**: Verify all 47 procedures save correctly
2. **Edit Log**: Ensure data persistence and validation
3. **Complete Log**: Test workflow and read-only enforcement
4. **Security**: Verify users cannot access others' logs
5. **Validation**: Test required fields and numerical constraints
6. **Navigation**: Confirm dashboard integration works properly
7. **Total Numbers**: Verify cumulative calculations are accurate across multiple months
8. **Analytics Performance**: Test with large datasets for performance

## Future Enhancements

### Potential Improvements
- **Advanced Analytics**: Trend analysis and monthly comparisons
- **Export**: PDF/Excel export functionality for analytics data
- **Templates**: Pre-filled templates for common scenarios
- **Bulk Operations**: Import/export multiple months
- **Audit Trail**: Track changes and completion history
- **Notifications**: Reminders for incomplete months
- **Goal Setting**: Target numbers for procedure completion
- **Benchmarking**: Compare against cohort averages (anonymized)

## Maintenance

### Regular Tasks
- Monitor database performance
- Review user feedback for UX improvements
- Update procedure lists as requirements change
- Backup clinical log data regularly

---

*Last Updated: August 5, 2025*
*Version: 2.0 - Added Total Numbers Analytics Feature*
*Author: Development Team*
