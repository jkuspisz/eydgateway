# EPA (Entrustable Professional Activity) Mapping Architecture

## 📋 Overview
The EPA Log serves as a central mapping system that connects portfolio activities to specific professional competencies. It allows EYD users to tag their portfolio entries with relevant EPAs, and enables ES/TPD users to track competency development across the entire portfolio.

## 🎯 Core Concept
- **EPA as Topics**: Each EPA represents a professional competency topic (e.g., "EPA 1: Assessing and managing new patients")
- **Portfolio Linking**: Any portfolio entry can be linked to one or more EPAs
- **Cross-Reference System**: The EPA Log shows where each EPA has been satisfied across different portfolio sections

## 🏗️ Technical Architecture

### Database Models

#### EPA Master List
```csharp
public class EPA
{
    public int Id { get; set; }
    public string Code { get; set; } // "EPA-1", "EPA-2", etc.
    public string Title { get; set; } // "Assessing and managing new patients"
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public List<EPAMapping> Mappings { get; set; } = new List<EPAMapping>();
}
```

#### EPA Mapping (Links portfolio items to EPAs)
```csharp
public class EPAMapping
{
    public int Id { get; set; }
    public int EPAId { get; set; }
    public string UserId { get; set; } // EYD User
    public string PortfolioItemType { get; set; } // "Reflection", "ClinicalLog", "SLE", etc.
    public int PortfolioItemId { get; set; }
    public string MappedBy { get; set; } // "EYD", "ES", "TPD"
    public DateTime MappedAt { get; set; }
    public string Notes { get; set; } // Optional mapping notes
    
    // Navigation properties
    public EPA EPA { get; set; }
    public ApplicationUser User { get; set; }
}
```

#### Portfolio Item Base (for linking)
```csharp
public interface IEPAMappable
{
    int Id { get; set; }
    string Title { get; set; }
    string UserId { get; set; }
    DateTime CreatedAt { get; set; }
    List<EPAMapping> EPAMappings { get; set; }
}

// Examples of portfolio items that implement IEPAMappable:
public class Reflection : IEPAMappable { /* ... */ }
public class ClinicalLogEntry : IEPAMappable { /* ... */ }
public class SLERecord : IEPAMappable { /* ... */ }
```

## 🔗 User Experience Flow

### For EYD Users - **Required EPA Selection**
When completing these specific portfolio activities, EYD users **MUST** select 1-2 EPAs:

#### Required EPA Mapping for:
1. **SLE (Supervised Learning Events)** - Select 1-2 EPAs
2. **Protected Learning Time** - Select 1-2 EPAs  
3. **Reflection** - Select 1-2 EPAs
4. **Significant Event Log** - Select 1-2 EPAs
5. **Quality Improvement Upload** - Select 1-2 EPAs

#### EPA Selection Interface (Required):
```html
<div class="epa-selection-required">
    <h6>Link to EPAs <span class="text-danger">*Required</span></h6>
    <p class="text-muted">Select 1-2 EPAs that this activity relates to:</p>
    <div class="epa-checkboxes">
        @foreach(var epa in Model.AvailableEPAs)
        {
            <div class="form-check">
                <input type="checkbox" name="SelectedEPAs" value="@epa.Id" 
                       class="epa-checkbox" required />
                <label>@epa.Code: @epa.Title</label>
            </div>
        }
    </div>
    <div class="epa-validation">
        <small class="text-muted">Please select between 1-2 EPAs to continue.</small>
    </div>
</div>
```

#### Form Validation:
- **Minimum**: 1 EPA must be selected
- **Maximum**: 2 EPAs can be selected
- **Error Messages**: 
  - "Please select at least 1 EPA"
  - "Please select no more than 2 EPAs"
- **Save Prevention**: Form cannot be submitted without EPA selection

### For ES/TPD Users
1. **EPA Log View**: Table showing EPA completion status:
   ```
   | EPA Code | EPA Title | Portfolio Evidence | Mapped Items | Status |
   |----------|-----------|-------------------|--------------|--------|
   | EPA-1 | Assessing new patients | 3 items | View Details | ✓ Complete |
   | EPA-2 | Clinical documentation | 1 item | View Details | ⚠ In Progress |
   ```

2. **Detail Drill-Down**: Click "View Details" to see:
   - List of linked portfolio items
   - Direct links to original content
   - Mapping history (who linked what when)

## 📊 EPA Log Dashboard

### Main EPA Log Table
- **Columns**: EPA Code, Title, Evidence Count, Latest Activity, Status
- **Filtering**: By status (Complete/In Progress/Not Started)
- **Sorting**: By EPA code, completion status, latest activity
- **Export**: PDF/Excel export for reviews

### EPA Detail View
- **Portfolio Items**: List all linked reflections, logs, SLEs, etc.
- **Timeline**: Chronological view of EPA development
- **Evidence Summary**: Quick overview of evidence types
- **Actions**: Add notes, mark as reviewed, flag for attention

### 🎨 UI Components

### EPA Selection Component (Required - 1-2 EPAs)
```csharp
@model EPASelectionViewModel
<div class="epa-selection-component required">
    <div class="alert alert-info">
        <i class="fas fa-info-circle me-2"></i>
        <strong>EPA Mapping Required:</strong> Select 1-2 EPAs that relate to this activity.
    </div>
    <div class="row">
        @foreach(var epa in Model.EPAs.Chunk(2))
        {
            <div class="col-md-6">
                @foreach(var item in epa)
                {
                    <div class="form-check mb-2">
                        <input type="checkbox" class="form-check-input epa-checkbox" 
                               name="SelectedEPAs" value="@item.Id"
                               data-epa-code="@item.Code"
                               @(Model.SelectedEPAIds.Contains(item.Id) ? "checked" : "") />
                        <label class="form-check-label">
                            <strong>@item.Code:</strong> @item.Title
                        </label>
                    </div>
                }
            </div>
        }
    </div>
    <div id="epa-validation-message" class="text-danger mt-2" style="display: none;">
        Please select between 1-2 EPAs to continue.
    </div>
</div>

<script>
document.addEventListener('DOMContentLoaded', function() {
    const checkboxes = document.querySelectorAll('.epa-checkbox');
    const validationMessage = document.getElementById('epa-validation-message');
    
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const checkedCount = document.querySelectorAll('.epa-checkbox:checked').length;
            
            if (checkedCount > 2) {
                this.checked = false;
                validationMessage.textContent = 'Maximum 2 EPAs can be selected.';
                validationMessage.style.display = 'block';
            } else if (checkedCount === 0) {
                validationMessage.textContent = 'Please select at least 1 EPA.';
                validationMessage.style.display = 'block';
            } else {
                validationMessage.style.display = 'none';
            }
        });
    });
});
</script>
```

### EPA Progress Badge
```html
<span class="epa-progress-badge badge bg-@(GetStatusColor(epa))">
    @epa.MappedItemsCount @(epa.MappedItemsCount == 1 ? "item" : "items")
</span>
```

## 🔄 Integration Points

### 1. Portfolio Forms
- Add EPA selection to all portfolio creation/edit forms
- Reflection forms
- Clinical log entry forms
- SLE recording forms
- Significant event logs

### 2. Dashboard Widgets
- **EYD Dashboard**: "Recent EPA Mappings" widget
- **ES Dashboard**: "Student EPA Progress" overview
- **TPD Dashboard**: "EPA Completion Statistics" across all EYDs

### 3. Reporting
- **Progress Reports**: EPA completion by EYD user
- **Competency Analysis**: Which EPAs need more focus
- **Portfolio Coverage**: Ensure balanced EPA mapping

## 📱 Implementation Phases

### Phase 1: Core EPA System
- [ ] Create EPA and EPAMapping models
- [ ] Set up database tables and relationships
- [ ] Create basic EPA CRUD operations
- [ ] Implement EPA selection component

### 2. Portfolio Integration
- [ ] Add **required** EPA mapping to Reflection forms (1-2 EPAs)
- [ ] Add **required** EPA mapping to SLE forms (1-2 EPAs)
- [ ] Add **required** EPA mapping to Protected Learning Time forms (1-2 EPAs)
- [ ] Add **required** EPA mapping to Significant Event forms (1-2 EPAs)
- [ ] Add **required** EPA mapping to Quality Improvement upload forms (1-2 EPAs)
- [ ] Implement client-side validation (1-2 EPA limit)
- [ ] Implement server-side validation (required EPA selection)

### Phase 3: EPA Log Dashboard
- [ ] Create EPA Log main table view
- [ ] Implement EPA detail drill-down
- [ ] Add filtering and sorting
- [ ] Create progress tracking visuals

### Phase 4: Advanced Features
- [ ] EPA progress analytics
- [ ] Automated EPA suggestions based on content
- [ ] Bulk EPA mapping tools
- [ ] Export and reporting features

## 🎯 Success Metrics
- **Portfolio Coverage**: % of portfolio items linked to EPAs
- **EPA Completion**: Number of EPAs with evidence per EYD user
- **User Adoption**: % of EYD users actively using EPA mapping
- **Supervisor Usage**: ES/TPD engagement with EPA log views

## 📋 Sample EPA List (for reference)
1. **EPA-1**: Assessing and managing new patients
2. **EPA-2**: Clinical documentation and record keeping
3. **EPA-3**: Handover communication
4. **EPA-4**: Emergency patient management
5. **EPA-5**: Procedural skills
6. **EPA-6**: Patient safety and quality improvement
7. **EPA-7**: Professional behavior and ethics
8. **EPA-8**: Teaching and mentoring
9. **EPA-9**: Research and evidence-based practice
10. **EPA-10**: Leadership and team working

*Note: Actual EPA list should be provided by medical education team*
