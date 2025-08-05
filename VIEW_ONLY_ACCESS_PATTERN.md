# View-Only Access Pattern for Portfolio Components

## Overview
This document outlines the view-only access pattern implemented for Reflections and Protected Learning Time (PLT) components, where EYD users have full CRUD access to their own content, while other users have view-only access based on their role permissions.

## Core Authorization Logic

### User Role Access Matrix
| User Role | Access Level | Restrictions |
|-----------|--------------|--------------|
| **EYD** | Full CRUD | Can only access their own content |
| **ES** | View Only | Can view assigned EYD users' content via EYDESAssignments |
| **Admin** | View Only | Can view any content |
| **Superuser** | View Only | Can view any content |

## Implementation Pattern

### 1. Controller Authorization Logic

#### Index Method Pattern
```csharp
public async Task<IActionResult> Index(string? id = null)
{
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser == null) return Challenge();

    string targetUserId = id ?? currentUser.Id;

    // Authorization check for viewing content
    if (currentUser.Role == "EYD")
    {
        // EYD users can only view their own content
        if (targetUserId != currentUser.Id)
        {
            return Forbid();
        }
    }
    else if (currentUser.Role == "ES")
    {
        // ES users can view content of their assigned EYD users
        var isAssigned = await _context.EYDESAssignments
            .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                     assignment.EYDUserId == targetUserId && 
                     assignment.IsActive);
        
        if (!isAssigned) return Forbid();
    }
    else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
    {
        return Forbid(); // Other roles are not allowed
    }

    // Get content for target user
    var content = await _context.YourContentTable
        .Where(c => c.UserId == targetUserId)
        .Include(c => c.User)
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync();

    // CRITICAL: Load EPA mappings for each item (for proper display in Index view)
    foreach (var item in content)
    {
        var epaMappings = await _context.EPAMappings
            .Include(em => em.EPA)
            .Where(em => em.EntityType == "YourEntityType" && em.EntityId == item.Id)
            .ToListAsync();
        item.EPAMappings = epaMappings;
    }

    // Set ViewBag properties for UI control
    var targetUser = await _userManager.FindByIdAsync(targetUserId);
    ViewBag.TargetUserName = targetUser?.UserName ?? "Unknown User";
    ViewBag.TargetUserId = targetUserId;
    
    // Check if current user can edit (only EYD users can edit their own content)
    ViewBag.CanEdit = currentUser.Role == "EYD" && targetUserId == currentUser.Id;

    return View(content);
}
```

#### Details Method Pattern
```csharp
public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();

    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser == null) return Challenge();

    // Get content without user restriction first
    var content = await _context.YourContentTable
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (content == null) return NotFound();

    // Check authorization based on user role
    if (currentUser.Role == "EYD")
    {
        // EYD users can only view their own content
        if (content.UserId != currentUser.Id)
        {
            return Forbid();
        }
    }
    else if (currentUser.Role == "ES")
    {
        // ES users can view content of their assigned EYD users
        var isAssigned = await _context.EYDESAssignments
            .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                     assignment.EYDUserId == content.UserId && 
                     assignment.IsActive);
        
        if (!isAssigned) return Forbid();
    }
    else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
    {
        return Forbid(); // Other roles are not allowed
    }

    // CRITICAL: Load EPA mappings for Details view
    var epaMappings = await _context.EPAMappings
        .Include(em => em.EPA)
        .Where(em => em.EntityType == "YourEntityType" && em.EntityId == content.Id)
        .ToListAsync();
    content.EPAMappings = epaMappings;

    // Set ViewBag properties for UI control
    ViewBag.CanEdit = currentUser.Role == "EYD" && content.UserId == currentUser.Id;
    ViewBag.TargetUserId = content.UserId;

    return View(content);
}
```

#### Create Method Pattern (GET)
```csharp
public async Task<IActionResult> Create(string? id = null)
{
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser == null) return Challenge();

    // EYD users can only create content for themselves
    string targetUserId = id ?? currentUser.Id;
    if (currentUser.Role != "EYD" || targetUserId != currentUser.Id)
    {
        TempData["ErrorMessage"] = "Only EYD users can create content for themselves.";
        return RedirectToAction("Index", new { id = targetUserId });
    }

    ViewBag.TargetUserId = targetUserId;
    
    // CRITICAL: Load EPAs for Create form
    ViewBag.EPAs = await _context.EPAs
        .Where(e => e.IsActive)
        .OrderBy(e => e.Title)
        .ToListAsync();

    var viewModel = new CreateYourViewModel();
    return View(viewModel);
}
```

#### Create Method Pattern (POST)
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateYourViewModel viewModel, string? id = null)
{
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser == null) return Challenge();

    string targetUserId = id ?? currentUser.Id;
    if (currentUser.Role != "EYD" || targetUserId != currentUser.Id)
    {
        TempData["ErrorMessage"] = "Only EYD users can create content for themselves.";
        return RedirectToAction("Index", new { id = targetUserId });
    }

    if (ModelState.IsValid)
    {
        var entity = new YourModel
        {
            // Map properties from viewModel
            UserId = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsLocked = false
        };

        _context.Add(entity);
        await _context.SaveChangesAsync();

        // CRITICAL: Create EPA mappings with proper UserId
        if (viewModel.SelectedEPAIds != null && viewModel.SelectedEPAIds.Any())
        {
            foreach (var epaId in viewModel.SelectedEPAIds)
            {
                var epaMapping = new EPAMapping
                {
                    EPAId = epaId,
                    EntityType = "YourEntityType",
                    EntityId = entity.Id,
                    UserId = currentUser.Id, // CRITICAL: Must set UserId for FK constraint
                    CreatedAt = DateTime.UtcNow
                };
                _context.EPAMappings.Add(epaMapping);
            }
            await _context.SaveChangesAsync();
        }
        
        TempData["SuccessMessage"] = "Content created successfully.";
        return RedirectToAction("Index", new { id = currentUser.Id });
    }

    ViewBag.TargetUserId = targetUserId;
    ViewBag.EPAs = await _context.EPAs
        .Where(e => e.IsActive)
        .OrderBy(e => e.Title)
        .ToListAsync();

    return View(viewModel);
}
```

#### Edit Method Pattern (POST)
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, EditYourViewModel viewModel)
{
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser == null) return Challenge();

    var existingEntity = await _context.YourContentTable.FindAsync(id);
    if (existingEntity == null) return NotFound();

    if (currentUser.Role != "EYD" || existingEntity.UserId != currentUser.Id)
    {
        TempData["ErrorMessage"] = "You can only edit your own content.";
        return RedirectToAction("Index", new { id = existingEntity.UserId });
    }

    if (ModelState.IsValid)
    {
        // Update properties from viewModel
        existingEntity.UpdatedAt = DateTime.UtcNow;

        _context.Update(existingEntity);
        await _context.SaveChangesAsync();

        // Update EPA mappings
        var existingMappings = await _context.EPAMappings
            .Where(em => em.EntityType == "YourEntityType" && em.EntityId == existingEntity.Id)
            .ToListAsync();
        _context.EPAMappings.RemoveRange(existingMappings);

        // CRITICAL: Create new EPA mappings with proper UserId
        if (viewModel.SelectedEPAIds != null && viewModel.SelectedEPAIds.Any())
        {
            foreach (var epaId in viewModel.SelectedEPAIds)
            {
                var epaMapping = new EPAMapping
                {
                    EPAId = epaId,
                    EntityType = "YourEntityType",
                    EntityId = existingEntity.Id,
                    UserId = currentUser.Id, // CRITICAL: Must set UserId for FK constraint
                    CreatedAt = DateTime.UtcNow
                };
                _context.EPAMappings.Add(epaMapping);
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Content updated successfully.";
        return RedirectToAction("Index", new { id = currentUser.Id });
    }

    ViewBag.TargetUserId = existingEntity.UserId;
    ViewBag.EPAs = await _context.EPAs
        .Where(e => e.IsActive)
        .OrderBy(e => e.Title)
        .ToListAsync();

    return View(viewModel);
}
```

#### GetAvailableEPAs Method Pattern (CRITICAL for EPA Loading)
```csharp
[HttpGet]
public async Task<IActionResult> GetAvailableEPAs()
{
    try
    {
        var epas = await _context.EPAs
            .Where(e => e.IsActive)
            .OrderBy(e => e.Title)
            .ToListAsync();
        var epaList = epas.Select(e => new
        {
            id = e.Id,
            code = e.Code,
            title = e.Title,
            description = e.Description
        }).ToList();

        return Json(new
        {
            success = true,
            epas = epaList,
            minSelections = 2
        });
    }
    catch (Exception ex)
    {
        return Json(new
        {
            success = false,
            message = "Error loading EPAs: " + ex.Message
        });
    }
}
```

### 2. View Template Patterns

#### Index View Pattern
```razor
@{
    ViewData["Title"] = "Content Index";
}

<div class="container-fluid">
    <div class="row">
        <div class="col-md-12">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <div>
                    <h2>@ViewData["Title"]</h2>
                    @if (ViewBag.TargetUserId != null && ViewBag.TargetUserId != User.FindFirst("Id")?.Value)
                    {
                        <p class="text-muted">
                            <i class="fas fa-user me-1"></i>Viewing content for: @ViewBag.TargetUserName
                        </p>
                    }
                </div>
                <div class="d-flex gap-2">
                    @if (ViewBag.CanEdit == false)
                    {
                        <span class="badge bg-info fs-6">
                            <i class="fas fa-eye me-1"></i>View Only
                        </span>
                    }
                    @if (ViewBag.CanEdit == true)
                    {
                        <a asp-action="Create" asp-route-id="@ViewBag.TargetUserId" class="btn btn-primary">
                            <i class="fas fa-plus me-2"></i>Add New Entry
                        </a>
                    }
                </div>
            </div>
        </div>
    </div>

    <!-- Content List -->
    @if (Model.Any())
    {
        <div class="row">
            @foreach (var item in Model)
            {
                <div class="col-md-6 col-lg-4 mb-4">
                    <div class="card h-100">
                        <div class="card-body">
                            <!-- Content display -->
                            <h5 class="card-title">@item.Title</h5>
                            <p class="card-text">@item.Description</p>
                            
                            <!-- CRITICAL: EPA Mappings Display -->
                            <p class="card-text">
                                <strong>EPA Mappings:</strong> 
                                @if (item.EPAMappings != null && item.EPAMappings.Any())
                                {
                                    <span class="badge bg-info">@item.EPAMappings.Count() EPAs</span>
                                }
                                else
                                {
                                    <span class="text-muted">No EPAs assigned</span>
                                }
                            </p>
                        </div>
                        <div class="card-footer">
                            <div class="d-flex justify-content-between">
                                <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-outline-primary btn-sm">
                                    <i class="fas fa-eye me-1"></i>View
                                </a>
                                @if (ViewBag.CanEdit == true)
                                {
                                    <div>
                                        <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-outline-secondary btn-sm me-1">
                                            <i class="fas fa-edit me-1"></i>Edit
                                        </a>
                                        <a asp-action="Delete" asp-route-id="@item.Id" class="btn btn-outline-danger btn-sm">
                                            <i class="fas fa-trash me-1"></i>Delete
                                        </a>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
    else
    {
        <div class="text-center mt-5">
            <i class="fas fa-content-icon fa-3x text-muted mb-3"></i>
            <h4 class="text-muted">No Content Found</h4>
            @if (ViewBag.CanEdit == true)
            {
                <p class="text-muted">Start by creating your first entry.</p>
                <a asp-action="Create" asp-route-id="@ViewBag.TargetUserId" class="btn btn-primary">
                    <i class="fas fa-plus me-2"></i>Add Your First Entry
                </a>
            }
            else
            {
                <p class="text-muted">This user hasn't created any entries yet.</p>
            }
        </div>
    }
</div>
```

#### Details View Pattern
```razor
@model YourModel

@{
    ViewData["Title"] = "Content Details";
}

<div class="container-fluid">
    <div class="row">
        <div class="col-md-8 offset-md-2">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <div>
                        <h4>@ViewData["Title"]</h4>
                        @if (ViewBag.TargetUserId != null && ViewBag.TargetUserId != User.FindFirst("Id")?.Value)
                        {
                            <small class="text-muted">
                                <i class="fas fa-user me-1"></i>Created by: @ViewBag.TargetUserName
                            </small>
                        }
                    </div>
                    <div class="d-flex gap-2">
                        @if (ViewBag.CanEdit == false)
                        {
                            <span class="badge bg-info">
                                <i class="fas fa-eye me-1"></i>View Only
                            </span>
                        }
                        <!-- Additional status badges -->
                    </div>
                </div>
                <div class="card-body">
                    <!-- Content display -->
                    <dl class="row">
                        <dt class="col-sm-3">Title</dt>
                        <dd class="col-sm-9">@Model.Title</dd>
                        
                        <dt class="col-sm-3">Description</dt>
                        <dd class="col-sm-9">@Model.Description</dd>
                        
                        <!-- CRITICAL: EPA Mappings Display -->
                        <dt class="col-sm-3">Related EPAs</dt>
                        <dd class="col-sm-9">
                            @if (Model.EPAMappings != null && Model.EPAMappings.Any())
                            {
                                <div class="row">
                                    @foreach (var mapping in Model.EPAMappings)
                                    {
                                        <div class="col-md-6 mb-2">
                                            <div class="card border-primary">
                                                <div class="card-body p-2">
                                                    <h6 class="card-title mb-1">
                                                        <span class="badge bg-primary me-2">@mapping.EPA.Code</span>
                                                        @mapping.EPA.Title
                                                    </h6>
                                                    @if (!string.IsNullOrEmpty(mapping.EPA.Description))
                                                    {
                                                        <p class="card-text small text-muted mb-0">@mapping.EPA.Description</p>
                                                    }
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                            }
                            else
                            {
                                <div class="alert alert-info">
                                    <i class="fas fa-info-circle me-2"></i>
                                    No EPA mappings have been assigned to this entry yet.
                                </div>
                            }
                        </dd>
                    </dl>
                    
                    <div class="d-flex justify-content-between">
                        <a asp-action="Index" asp-route-id="@ViewBag.TargetUserId" class="btn btn-secondary">Back to List</a>
                        @if (ViewBag.CanEdit == true)
                        {
                            <div>
                                <a asp-action="Edit" asp-route-id="@Model?.Id" class="btn btn-primary me-2">Edit</a>
                                <a asp-action="Delete" asp-route-id="@Model?.Id" class="btn btn-danger">Delete</a>
                            </div>
                        }
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### Create/Edit View JavaScript Pattern
```javascript
<script>
    // Store previously selected EPA IDs for validation error scenarios
    var previouslySelectedEPAIds = @Html.Raw(Json.Serialize(Model.SelectedEPAIds ?? new List<int>()));
    
    $(document).ready(function() {
        // Load EPAs immediately
        loadAvailableEPAs();
    });

    function loadAvailableEPAs() {
        console.log('Loading EPAs');
        $('#epa-selection-area').html('<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Loading available EPAs...</div>');
        
        $.ajax({
            url: '@Url.Action("GetAvailableEPAs", "YourController")',
            type: 'GET',
            success: function(response) {
                console.log('EPA AJAX response:', response);
                if (response.success) {
                    renderEPASelection(response.epas, response.minSelections);
                    updateEPARequirements(response.minSelections);
                } else {
                    console.error('EPA loading failed:', response.message);
                    $('#epa-selection-area').html('<div class="alert alert-danger">Error loading EPAs: ' + response.message + '</div>');
                }
            },
            error: function(xhr, status, error) {
                console.error('EPA AJAX error:', status, error, xhr.responseText);
                $('#epa-selection-area').html('<div class="alert alert-danger">Failed to load EPAs. Please try again. Error: ' + error + '</div>');
            }
        });
    }
</script>
```

## Critical Implementation Points

### 1. Authorization Flow
1. **Authentication Check**: Verify user is logged in
2. **Role-Based Access**: Determine access level based on user role  
3. **Relationship Validation**: For ES users, verify assignment via EYDESAssignments
4. **Permission Setting**: Set ViewBag.CanEdit for UI control

### 2. EPA Mapping Management
- **Index Method**: Must load EPA mappings for each item to display counts
- **Details Method**: Must load EPA mappings to show EPA details
- **Create/Edit Methods**: Must set `UserId` in EPA mappings for FK constraint
- **AJAX Endpoint**: Must return structured JSON with success/error handling

### 3. UI Permission Controls
- **ViewBag.CanEdit**: Boolean flag controlling edit/delete button visibility
- **View-Only Badge**: Visual indicator for non-editing users
- **Target User Display**: Show whose content is being viewed
- **Conditional Actions**: Show/hide Create, Edit, Delete buttons based on permissions

### 4. Navigation Patterns
- **Index Routes**: Include target user ID for proper authorization context
- **Back Navigation**: Maintain user context when returning to lists
- **Error Handling**: Return appropriate HTTP status codes (Forbid, NotFound)

### 5. Database Constraints
- **EPA Mappings**: Must include `UserId` field for foreign key constraint
- **Entity Types**: Use consistent naming ("Reflection", "ProtectedLearningTime")
- **Active Filter**: Only load active EPAs for selection

## Applied To
- ✅ **Reflections** (ReflectionController + Views)
- ✅ **Protected Learning Time** (ProtectedLearningTimeController + Views)

## Future Application
This pattern can be extended to:
- Learning Needs
- SLE entries  
- Any other portfolio components requiring view-only access for supervisors

## Testing Checklist
- [x] EYD user can view/edit their own content
- [x] EYD user cannot view other users' content
- [x] ES user can view assigned EYD users' content (view-only)
- [x] ES user cannot view unassigned EYD users' content
- [x] ES user cannot edit any content
- [x] Admin/Superuser can view all content (view-only)
- [x] UI properly shows/hides action buttons based on permissions
- [x] View-only badges display correctly
- [x] Navigation maintains proper user context
- [x] EPA mappings display correctly in Index and Details views
- [x] EPA creation/editing works without FK constraint errors
- [x] AJAX EPA loading works properly in Create/Edit forms

## Troubleshooting Common Issues

### 1. EPA Mappings Not Displaying
**Problem**: EPAs show as "No EPAs assigned" even when they should exist  
**Solution**: Ensure EPA mappings are loaded in Index and Details methods with proper Include statements

### 2. Foreign Key Constraint Violations  
**Problem**: `EPAMappings` table FK constraint error when creating/editing  
**Solution**: Always set `UserId = currentUser.Id` when creating EPAMapping entities

### 3. AJAX EPA Loading Fails
**Problem**: EPA selection not loading in Create/Edit forms  
**Solution**: Ensure GetAvailableEPAs returns structured JSON with `{success: true, epas: [...]}` format

### 4. Authorization Logic Errors
**Problem**: Users getting unexpected Forbid responses  
**Solution**: Use proper if/else logic structure to avoid unintended authorization blocks

## Recent Fixes Applied

### Latest Resolution (Database Constraint Issue)
- **Issue**: PostgreSQL foreign key constraint violation in EPAMappings table
- **Root Cause**: Missing `UserId` field in EPA mapping creation
- **Solution**: Added `UserId = currentUser.Id` to EPA mapping creation in both Create and Edit methods
- **Reference**: Used ProtectedLearningTime controller as working template

### Systematic Debugging Approach
1. **Compare Working Implementation**: Used PLT controller as reference for EPA handling
2. **Identify Missing Components**: Found EPA loading methods and UserId assignment patterns
3. **Apply Consistent Patterns**: Matched Reflection controller to PLT controller exactly
4. **Verify Database Integrity**: Ensured all foreign key constraints satisfied

This pattern is now fully implemented and tested across both Reflection and PLT systems with complete EPA functionality.
