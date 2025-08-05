# ES Portfolio Access Fix Documentation

## Problem Summary
Educational Supervisor (ES) users were unable to properly view and manage portfolio content of their assigned EYD (Early Years Dentist) users. The issue manifested in two ways:
1. ES users saw their own empty portfolio data instead of the target EYD user's data
2. ES users got 404 errors when trying to view individual portfolio items (reflections, learning needs, PLT entries)

## Root Cause Analysis

### Issue 1: Parameter Binding Mismatch
- **Problem**: ASP.NET Core default routing uses `{controller}/{action}/{id?}` pattern
- **Conflict**: Portfolio controllers expected `userId` parameter but routing provided `id` parameter
- **Result**: ES users saw empty portfolios because `userId` was null/empty

### Issue 2: Navigation Parameter Format
- **Problem**: Portfolio page used query parameters (`/Controller/Action?userId=xxx`)
- **Conflict**: Updated controllers expected route parameters (`/Controller/Action/xxx`)
- **Result**: Portfolio section navigation didn't pass user context correctly

### Issue 3: Authorization Logic
- **Problem**: Individual item controllers only checked `item.UserId == currentUser.Id`
- **Conflict**: ES users needed access to items belonging to their assigned EYD users
- **Result**: 404 errors when ES users tried to view specific portfolio items

## Complete Solution

### 1. Fixed Parameter Binding (Controllers)

**Updated Controller Method Signatures:**
```csharp
// BEFORE
public async Task<IActionResult> Index(string? userId = null)

// AFTER  
public async Task<IActionResult> Index(string? id = null)
```

**Controllers Updated:**
- `EYDController.cs` - Portfolio method and all action methods
- `ReflectionController.cs` - Index method
- `LearningNeedController.cs` - Index method
- `ProtectedLearningTimeController.cs` - Index method
- `SLEController.cs` - Index method

**Internal Logic Updated:**
```csharp
// BEFORE
string targetUserId = userId ?? currentUser.Id;

// AFTER
string targetUserId = id ?? currentUser.Id;
```

### 2. Fixed Portfolio Navigation (JavaScript)

**File:** `Views/EYD/Portfolio.cshtml`

```javascript
// BEFORE - Query Parameters
function navigateToSection(controller, action, parameters, userId) {
    var url = '/' + controller + '/' + action;
    if (parameters) url += '?' + parameters;
    if (userId) url += (hasParams ? '&' : '?') + 'userId=' + userId;
    window.location.href = url;
}

// AFTER - Route Parameters
function navigateToSection(controller, action, parameters, userId) {
    // Default to EYD controller if no controller specified
    if (!controller || controller === '') {
        controller = 'EYD';
    }
    
    var url = '/' + controller + '/' + action;
    
    // Add userId as route parameter
    if (userId && userId !== '') {
        url += '/' + userId;
    }
    
    // Add any additional parameters as query parameters
    if (parameters && parameters !== '') {
        url += '?' + parameters;
    }
    
    window.location.href = url;
}
```

### 3. Fixed Individual Item Authorization

**Updated Authorization Logic for Details/Edit/Delete Methods:**

```csharp
// BEFORE - Only current user access
var item = await _context.Items
    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == currentUser.Id);

// AFTER - ES user support
// Step 1: Get item without user restriction
var item = await _context.Items
    .FirstOrDefaultAsync(i => i.Id == id);

if (item == null) return NotFound();

// Step 2: Check authorization based on user role
if (currentUser.Role == "EYD" && item.UserId != currentUser.Id)
{
    return Forbid(); // EYD users can only access their own items
}
else if (currentUser.Role == "ES")
{
    // ES users can access items of their assigned EYD users
    var isAssigned = await _context.EYDESAssignments
        .AnyAsync(assignment => assignment.ESUserId == currentUser.Id && 
                 assignment.EYDUserId == item.UserId && 
                 assignment.IsActive);
    
    if (!isAssigned) return Forbid();
}
else if (currentUser.Role != "Admin" && currentUser.Role != "Superuser")
{
    return Forbid(); // Other roles are not allowed
}
```

**Controllers Updated:**
- `ReflectionController.cs` - Details, Edit, Delete, DeleteConfirmed methods
- `LearningNeedController.cs` - Details, Edit methods
- `ProtectedLearningTimeController.cs` - Details, Edit methods

## Navigation Flow (Now Working)

```
ES Dashboard 
    ↓ Click "View Dashboard" 
EYD Portfolio (shows target EYD user's data)
    ↓ Click "Reflection" section
Reflection Index (shows target EYD user's reflections) 
    ↓ Click individual reflection
Reflection Details (shows target EYD user's reflection)
    ↓ Click "Edit"
Reflection Edit (can edit target EYD user's reflection)
```

## URL Structure

### Before Fix
```
ES Dashboard → /EYD/Portfolio/2e2f1413-f875-43fd-a43d-d7e3ed196e04
Portfolio → /Reflection/Index?userId=2e2f1413-f875-43fd-a43d-d7e3ed196e04
Individual → /Reflection/Details/5 (fails - wrong user context)
```

### After Fix
```
ES Dashboard → /EYD/Portfolio/2e2f1413-f875-43fd-a43d-d7e3ed196e04  
Portfolio → /Reflection/Index/2e2f1413-f875-43fd-a43d-d7e3ed196e04
Individual → /Reflection/Details/5 (works - proper ES authorization)
```

## Security Model

### Authorization Matrix
| User Role | Own Data | Assigned EYD Data | Other Data | Admin Override |
|-----------|----------|-------------------|------------|----------------|
| EYD       | ✅ Full  | ❌ None           | ❌ None    | ❌ None        |
| ES        | ✅ Full  | ✅ Full*          | ❌ None    | ❌ None        |
| Admin     | ✅ Full  | ✅ Full           | ✅ Full    | ✅ Full        |
| Superuser | ✅ Full  | ✅ Full           | ✅ Full    | ✅ Full        |

*ES users can only access data from EYD users they are assigned to supervise (active assignments in `EYDESAssignments` table)

### Assignment Validation
```csharp
var isAssigned = await _context.EYDESAssignments
    .AnyAsync(assignment => 
        assignment.ESUserId == currentUser.Id && 
        assignment.EYDUserId == targetUserId && 
        assignment.IsActive);
```

## Files Modified

### Controllers
- `Controllers/EYDController.cs` - Parameter names, redirect logic
- `Controllers/ReflectionController.cs` - Index, Details, Edit, Delete methods
- `Controllers/LearningNeedController.cs` - Index, Details, Edit methods  
- `Controllers/ProtectedLearningTimeController.cs` - Index, Details, Edit methods
- `Controllers/SLEController.cs` - Index method

### Views
- `Views/EYD/Portfolio.cshtml` - JavaScript navigation function

## Testing Checklist

### ES User Navigation
- [ ] ES Dashboard shows assigned EYD users
- [ ] Click "View Dashboard" shows target EYD portfolio (not ES portfolio)
- [ ] Portfolio sections show target EYD data counts
- [ ] Click "Reflection" shows target EYD reflections
- [ ] Click "Learning Needs" shows target EYD learning needs
- [ ] Click "Protected Learning Time" shows target EYD PLT entries
- [ ] Click "SLEs" shows target EYD SLE entries

### ES User Item Access
- [ ] Click individual reflection shows target EYD reflection details
- [ ] Click "Edit" on reflection allows editing target EYD reflection
- [ ] Click individual learning need shows target EYD learning need details
- [ ] Click individual PLT entry shows target EYD PLT details
- [ ] All authorization checks prevent access to non-assigned EYD data

### EYD User Access (Regression Test)
- [ ] EYD users can access their own portfolio normally
- [ ] EYD users cannot access other EYD users' data
- [ ] All existing EYD functionality works as before

## Debug Information

### Browser Console Logs
```javascript
// ES Dashboard navigation
DEBUG: viewEYDDashboard called with eydId: 2e2f1413-f875-43fd-a43d-d7e3ed196e04
DEBUG: Navigating to: /EYD/Portfolio/2e2f1413-f875-43fd-a43d-d7e3ed196e04

// Portfolio section navigation  
DEBUG: navigateToSection - Controller: Reflection Action: Index UserId: 2e2f1413-f875-43fd-a43d-d7e3ed196e04
DEBUG: navigateToSection - Final URL: /Reflection/Index/2e2f1413-f875-43fd-a43d-d7e3ed196e04
```

### Server-Side Debug Logs
```csharp
Console.WriteLine($"DEBUG Portfolio: User '{currentUser.UserName}' accessing portfolio for user ID: '{id}'");
Console.WriteLine($"DEBUG Portfolio: Created ViewModel for {viewModel.UserName} (UserID: {viewModel.UserId})");
```

## Known Issues Resolved

1. ✅ **401 Unauthorized errors** - Fixed with ES authorization in Portfolio controller
2. ✅ **Empty portfolio data** - Fixed with parameter binding from `userId` to `id`
3. ✅ **404 errors on individual items** - Fixed with ES authorization in item controllers
4. ✅ **Navigation not passing user context** - Fixed with route parameter format
5. ✅ **Wrong portfolio data showing** - Fixed with proper parameter passing

## Maintenance Notes

- When adding new portfolio controllers, ensure they follow the same authorization pattern
- When adding new portfolio sections, ensure they use the correct navigation format
- All portfolio-related controllers should use `id` parameter (not `userId`) for consistency with ASP.NET Core routing
- ES authorization checks should always verify active assignments in `EYDESAssignments` table
