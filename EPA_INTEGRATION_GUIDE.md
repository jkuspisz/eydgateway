# EPA Selection Components - Usage Guide

## Overview
This guide shows how to integrate the reusable EPA selection components into any portfolio form across the EYD platform.

## Components Created
1. **_EPASelection.cshtml** - Reusable partial view with styling
2. **epa-selection.js** - JavaScript validation and interaction logic  
3. **EPAService.cs** - Backend service for EPA operations

## Quick Integration Steps

### 1. Controller Setup
```csharp
// In your controller (e.g., ReflectionController, SLEController)
public class ReflectionController : Controller
{
    private readonly IEPAService _epaService;
    
    public ReflectionController(IEPAService epaService)
    {
        _epaService = epaService;
    }
    
    public async Task<IActionResult> Create()
    {
        ViewBag.EPAs = await _epaService.GetAllActiveEPAsAsync();
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(ReflectionViewModel model)
    {
        // Validate EPA selection
        if (!await _epaService.ValidateEPASelectionAsync(model.SelectedEPAIds))
        {
            ModelState.AddModelError("SelectedEPAIds", "Please select 1-2 EPAs");
            ViewBag.EPAs = await _epaService.GetAllActiveEPAsAsync();
            return View(model);
        }
        
        // Save your main entity first
        var reflection = new Reflection { /* your properties */ };
        _context.Reflections.Add(reflection);
        await _context.SaveChangesAsync();
        
        // Save EPA mappings
        await _epaService.SaveEPAMappingAsync("Reflection", reflection.Id, model.SelectedEPAIds, User.Identity.Name);
        
        return RedirectToAction("Index");
    }
}
```

### 2. View Integration
```html
<!-- In your form view (e.g., Create.cshtml) -->
<form asp-action="Create" method="post">
    <!-- Your existing form fields -->
    <div class="mb-3">
        <label asp-for="Title" class="form-label">Title</label>
        <input asp-for="Title" class="form-control" />
    </div>
    
    <!-- EPA Selection Component -->
    @await Html.PartialAsync("_EPASelection", ViewBag.EPAs as IEnumerable<EPA>)
    
    <!-- Hidden field to capture selected EPA IDs -->
    <input type="hidden" name="SelectedEPAIds" id="selectedEPAIds" />
    
    <button type="submit" class="btn btn-primary">Save</button>
</form>

<!-- Include EPA selection JavaScript -->
<script src="~/js/epa-selection.js"></script>

<!-- Update form submission to include EPA IDs -->
<script>
document.querySelector('form').addEventListener('submit', function() {
    // Get selected EPA IDs and populate hidden field
    const selectedIds = window.epaSelection.getSelectedEPAIds();
    document.getElementById('selectedEPAIds').value = selectedIds.join(',');
});
</script>
```

### 3. ViewModel Updates
```csharp
public class ReflectionViewModel
{
    public string Title { get; set; }
    public string Content { get; set; }
    // Add EPA selection property
    public List<int> SelectedEPAIds { get; set; } = new List<int>();
}
```

### 4. Register EPA Service
```csharp
// In Program.cs or Startup.cs
builder.Services.AddScoped<IEPAService, EPAService>();
```

## Usage in Different Portfolio Sections

### SLE Forms
- Entity Type: "SLE"
- Include SLE type selection + EPA mapping
- Same integration pattern

### Protected Learning Time
- Entity Type: "ProtectedLearningTime" 
- Same integration pattern

### Significant Events
- Entity Type: "SignificantEvent"
- Same integration pattern

### Quality Improvement Uploads
- Entity Type: "QIUpload"
- Same integration pattern

## Features Included

✅ **Automatic Validation**: 1-2 EPA selection enforced
✅ **Visual Feedback**: Real-time selection count and styling
✅ **Form Integration**: Prevents submission with invalid selection
✅ **Reusable**: Same components work across all portfolio sections
✅ **Responsive Design**: Works on mobile and desktop
✅ **Accessibility**: Proper labels and keyboard navigation

## Testing the Components

1. **Navigate to any form with EPA selection**
2. **Try selecting 0 EPAs** - Should show validation error
3. **Try selecting 3+ EPAs** - Should prevent and show warning  
4. **Select 1-2 EPAs** - Should show success state and allow submission
5. **Submit form** - Should save EPA mappings correctly

## EPA Integration Checklist for New Portfolio Sections

When expanding any portfolio section, use this comprehensive checklist to ensure EPA tracking continuity:

### Required Portfolio Sections for EPA Mapping
These sections **MUST** include EPA selection:
- ✅ **EPA Matrix** (already implemented)
- ✅ **SLE (Supervised Learning Events)** - All 6 types implemented with dynamic EPA validation
- 🔄 **Reflection forms**
- 🔄 **Protected Learning Time**
- 🔄 **Significant Event Log**
- 🔄 **Quality Improvement uploads**

### Controller Pattern to Follow
```csharp
// Always inject EPA service in constructor
private readonly IEPAService _epaService;

public YourController(ApplicationDbContext context, IEPAService epaService)
{
    _context = context;
    _epaService = epaService; // DON'T FORGET THIS
}

// For create forms - load EPAs
public async Task<IActionResult> Create()
{
    ViewBag.EPAs = await _epaService.GetAllActiveEPAsAsync(); // CRITICAL
    return View();
}

// For save actions - validate and save EPA mappings
[HttpPost]
public async Task<IActionResult> Create(YourViewModel model)
{
    // 1. Validate EPA selection (1-2 EPAs required)
    if (!await _epaService.ValidateEPASelectionAsync(model.SelectedEPAIds))
    {
        ModelState.AddModelError("SelectedEPAIds", "Please select 1-2 EPAs");
        ViewBag.EPAs = await _epaService.GetAllActiveEPAsAsync();
        return View(model);
    }
    
    // 2. Save your main entity
    var entity = new YourEntity { /* properties */ };
    _context.YourEntities.Add(entity);
    await _context.SaveChangesAsync();
    
    // 3. Save EPA mappings - DON'T FORGET THIS STEP
    await _epaService.SaveEPAMappingAsync(
        "YourEntityType", // e.g., "Reflection", "SLE", "ProtectedLearningTime"
        entity.Id,
        model.SelectedEPAIds,
        User.Identity.Name
    );
    
    return RedirectToAction("Index");
}
```

### View Integration Pattern
Every form view needs:
```html
<!-- Include EPA selection partial -->
@await Html.PartialAsync("_EPASelection", new { EPAs = ViewBag.EPAs })

<!-- Your other form fields here -->

<!-- Include EPA validation script -->
<script src="~/js/epa-selection.js"></script>
```

### ViewModel Requirements
```csharp
public class YourFormViewModel
{
    // Your existing properties...
    
    // REQUIRED: EPA selection property
    public List<int> SelectedEPAIds { get; set; } = new List<int>();
}
```

### EPA Service Methods Available
The following methods are already implemented in `EPAService.cs`:
```csharp
// Get all EPAs for selection
await _epaService.GetAllActiveEPAsAsync()

// Validate 1-2 EPA selection
await _epaService.ValidateEPASelectionAsync(selectedEPAIds)

// Save EPA mappings after entity creation
await _epaService.SaveEPAMappingAsync(entityType, entityId, epaIds, userId)

// Get existing mappings for edit forms
await _epaService.GetEPAMappingsAsync(entityType, entityId)
```

### Database Integration Points
The EPA Matrix automatically picks up new mappings via:
```csharp
// This query in EPA() action finds all user EPA mappings
var userEPAMappings = await _context.EPAMappings
    .Include(m => m.EPA)
    .Where(m => m.UserId == currentUser.Id)
    .GroupBy(m => new { m.EPAId, m.EntityType })
```

**Key**: Use consistent `EntityType` names:
- `"Reflection"` for reflection entries
- `"SLE"` for supervised learning events  
- `"ProtectedLearningTime"` for protected learning time
- `"SignificantEvent"` for significant events
- `"QIUpload"` for quality improvement uploads

### Files to Reference
When implementing new sections, check these existing files:
- **Integration Guide**: `EPA_INTEGRATION_GUIDE.md` (this file)
- **Partial View**: `Views/Shared/_EPASelection.cshtml` 
- **JavaScript**: `wwwroot/js/epa-selection.js`
- **Service Interface**: `Services/IEPAService.cs`
- **Service Implementation**: `Services/EPAService.cs`
- **Working Example**: `Controllers/EYDController.cs` EPA() action

### Testing Checklist
For each new section:
- [ ] EPA selection component appears on create/edit forms
- [ ] JavaScript validation prevents 0 or 3+ EPA selections  
- [ ] Form submission blocked without 1-2 EPAs selected
- [ ] EPA mappings saved to database after entity creation
- [ ] New activities appear in EPA Matrix automatically
- [ ] Edit forms pre-populate existing EPA selections

## Next Steps

Ready to implement in:
1. ✅ **SLE section** - Complete! All 6 types with EPA mapping and validation
2. 🔄 **Reflection forms**
3. 🔄 **Protected Learning Time forms**
4. 🔄 **Any other portfolio section**

The EPA components are now ready for integration across the entire EYD platform!
