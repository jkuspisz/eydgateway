# EYD Gateway Platform - Implementation Plan
## Corrected User Roles & Database Structure

---

## 🎯 **CURRENT STATE vs TARGET STATE**

### **COMPLETED IMPLEMENTATIONS (August 2025):**
- ✅ **Enhanced TPD/Dean Dashboard** → View all schemes in their area + cross-area EYD search
- ✅ **Area-Restricted Viewing** → TPDs can view EYD users in any scheme within their area
- ✅ **Cross-Area Search** → Exact username/GDC number search across all areas
- ✅ **Security Boundaries** → Proper area validation and search-only access
- ✅ **ES-EYD Assignment System** → Direct ES-EYD supervision relationships
- ✅ **Role Separation** → TPD and Dean treated as distinct roles
- ✅ **View-Only Permissions** → EYD assignment restricted to Admin/Superuser only

### **CURRENT ISSUES (LEGACY PLANNING):**
- ❌ EYDs assigned to Areas (should be assigned to Schemes)
- ❌ TPDs assigned to Areas (should be assigned to Schemes - 1:1 relationship)
- ❌ No ES-to-EYD assignment system (missing many-to-many)
- ❌ No cross-area search for TPDs/Deans
- ❌ No temporary access system

### **TARGET STATE:**
- ✅ **Superuser** → Assigns Admin to Area
- ✅ **Admin** → Assigns EYDs to Schemes (within their Area)
- ✅ **TPD** → Assigned to ONE Scheme (1:1 relationship)
- ✅ **Dean** → Usually NOT assigned to Scheme (but can search/access)
- ✅ **ES** → Assigned to specific EYDs (many-to-many)
- ✅ **EYD** → Assigned to ONE Scheme
- ✅ **Cross-area search** for TPDs/Deans
- ✅ **Temporary access** system for out-of-area EYDs

---

## 📋 **STEP-BY-STEP IMPLEMENTATION PLAN**

### **PHASE 1: Database Structure Updates**

#### **Step 1.1: Update ApplicationUser Model**
**File:** `Models/ApplicationUser.cs`
**Changes:**
```csharp
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; }
    public string Role { get; set; }
    
    // Area assignment (for Admins mainly)
    public int? AreaId { get; set; }
    public Area? Area { get; set; }
    
    // NEW: Scheme assignment (for EYDs and TPDs)
    public int? SchemeId { get; set; }
    public Scheme? Scheme { get; set; }
}
```

#### **Step 1.2: Create ES-EYD Assignment Model**
**File:** `Models/EYDESAssignment.cs` (NEW FILE)
**Content:**
```csharp
public class EYDESAssignment
{
    public int Id { get; set; }
    public string EYDUserId { get; set; }
    public string ESUserId { get; set; }
    public DateTime AssignedDate { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ApplicationUser EYDUser { get; set; }
    public ApplicationUser ESUser { get; set; }
}
```

#### **Step 1.3: Create Temporary Access Model**
**File:** `Models/TemporaryAccess.cs` (NEW FILE)
**Content:**
```csharp
public class TemporaryAccess
{
    public int Id { get; set; }
    public string RequestingUserId { get; set; }  // TPD/Dean requesting access
    public string TargetEYDUserId { get; set; }   // EYD they want to access
    public string Reason { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public string? ApprovedByUserId { get; set; }
    
    // Navigation properties
    public ApplicationUser RequestingUser { get; set; }
    public ApplicationUser TargetEYDUser { get; set; }
    public ApplicationUser? ApprovedByUser { get; set; }
}
```

#### **Step 1.4: Update ApplicationDbContext**
**File:** `Data/ApplicationDbContext.cs`
**Add DbSets:**
```csharp
public DbSet<EYDESAssignment> EYDESAssignments { get; set; }
public DbSet<TemporaryAccess> TemporaryAccesses { get; set; }
```

#### **Step 1.5: Create and Apply Migration**
**Terminal Commands:**
```bash
dotnet ef migrations add UpdateUserSchemeAssignments
dotnet ef database update
```

---

### **PHASE 2: Update User Assignment Logic**

#### **Step 2.1: Update Setup Demo Data**
**File:** `Program.cs` - `/setup-demo-data` endpoint
**Logic:**
- Assign TPDs to specific Schemes (1:1) - each Scheme is a subdivision of an Area
- Assign EYDs to Schemes (not Areas) - Schemes group EYDs within an Area
- Keep Admins assigned to Areas
- Leave Deans without Scheme assignment
- Create sample ES-EYD assignments

**Example Structure:**
- Area: "North West England"
  - Scheme: "North West Group A" (TPD: John Smith, EYDs: 10-15 trainees)
  - Scheme: "North West Group B" (TPD: Jane Doe, EYDs: 10-15 trainees)
- Area: "South East England" 
  - Scheme: "South East Group A" (TPD: Bob Wilson, EYDs: 12-18 trainees)

#### **Step 2.2: Update Admin Controller**
**File:** `Controllers/AdminController.cs`
**Changes:**
- Add Scheme management within Admin's Area
  - **Create new schemes** (e.g., yearly cohorts: "North West Group A 2025", "North West Group A 2026")
  - **Edit existing schemes** (name, status, archive old ones)
  - **Delete/Archive schemes** when cohorts graduate
- Add EYD-to-Scheme assignment functionality
- Add TPD-to-Scheme assignment functionality (1:1 relationship)
- Add ES-to-EYD assignment functionality (many-to-many via EYDESAssignment table)
- Update user creation to handle Scheme assignments
- **Admin Dashboard enhancements:**
  - Scheme overview within their assigned Area
  - User assignment tools
  - Bulk operations for moving users between schemes

---

### **PHASE 3: Search & Access Features**

#### **Step 3.1: Create EYD Search Controller**
**File:** `Controllers/SearchController.cs` (NEW FILE)
**Features:**
- Cross-area EYD search for TPDs/Deans
- Filter by Area, Scheme, name, etc.
- View-only access to search results

#### **Step 3.2: Create Temporary Access Controller**
**File:** `Controllers/AccessController.cs` (NEW FILE)
**Features:**
- Request temporary access to EYDs
- Approve/deny access requests (by Admins/Superuser)
- View active temporary access grants
- Auto-expire based on date

#### **Step 3.3: Update TPD/Dean Dashboards**
**Files:** 
- `Controllers/TPDController.cs`
- `Views/TPD/Dashboard.cshtml`
**Add:**
- Search functionality
- Temporary access request form
- List of current temporary access grants

---

### **PHASE 4: Update All Controllers**

#### **Step 4.1: Update TPD Controller**
**File:** `Controllers/TPDController.cs`
**Changes:**
- Use SchemeId instead of AreaId for TPD assignment
- Show only EYDs in TPD's assigned Scheme
- Add search and temporary access features

#### **Step 4.2: Update EYD Controller**
**File:** `Controllers/EYDController.cs`
**Changes:**
- Use SchemeId for EYD assignment
- Show Scheme information instead of Area
- Update navigation and breadcrumbs

#### **Step 4.3: Update ES Controller**
**File:** `Controllers/ESController.cs`
**Changes:**
- Use EYDESAssignment table for assignments
- Show only assigned EYDs
- Handle multiple EYD assignments per ES

---

### **PHASE 5: Update Views & UI**

#### **Step 5.1: Update Navigation**
**File:** `Views/Shared/_Layout.cshtml`
**Add:**
- Search link for TPDs/Deans
- Access requests link
- Scheme information in breadcrumbs

#### **Step 5.2: Create Search Views**
**Files:** (NEW)
- `Views/Search/Index.cshtml`
- `Views/Search/Results.cshtml`

#### **Step 5.3: Create Access Request Views**
**Files:** (NEW)
- `Views/Access/Request.cshtml`
- `Views/Access/Manage.cshtml`
- `Views/Access/MyRequests.cshtml`

---

## 🔧 **IMPLEMENTATION CHECKLIST**

### **Database Changes:**
- [x] Update ApplicationUser with SchemeId
- [x] Create EYDESAssignment model
- [x] Create TemporaryAccess model
- [x] Update ApplicationDbContext
- [x] Create and apply migration

### **Assignment Logic:**
- [x] Update setup demo data for correct assignments
- [ ] Update Admin controller for Scheme management
- [ ] Add Scheme creation/editing functionality for Admins
- [ ] Add yearly cohort management (create new schemes per year)
- [ ] Update user assignment workflows
- [x] Create ES-to-EYD assignment system

### **Search & Access:**
- [ ] Create SearchController
- [ ] Create AccessController
- [ ] Add search views
- [ ] Add access request views

### **Controller Updates:**
- [ ] Update TPDController for Scheme-based access
- [ ] Update EYDController for Scheme assignment
- [ ] Update ESController for EYD assignments

### **UI Updates:**
- [ ] Update navigation
- [ ] Add search functionality
- [ ] Add access request UI

---

## 🎯 **EXECUTION ORDER**

1. **Start with Phase 1** (Database) - Foundation changes
2. **Phase 2** (Assignment Logic) - Fix current assignments
3. **Test basic functionality** - Ensure TPDs see correct EYDs
4. **Phase 3** (Search & Access) - Add new features
5. **Phase 4** (Controllers) - Update existing functionality
6. **Phase 5** (Views) - Polish UI

---

## 📝 **NOTES FOR IMPLEMENTATION**

### **Key Concept - What is a "Scheme":**
- **Scheme = Geographic/Administrative subdivision of an Area**
- NOT a dental specialty or clinical program
- Purpose: Split large Areas into manageable groups for TPDs
- Example: "North West England" → "North West Group A", "North West Group B"
- Each Scheme has one TPD managing 10-20 EYDs
- **Yearly Management**: New schemes created annually (e.g., "North West Group A 2025", "North West Group A 2026")
- **Admin Control**: Area Admins can create, edit, and archive schemes within their area

### **User Assignment Hierarchy (Corrected):**
- **Superuser** → Assigns Admin to Area → Admin creates Schemes → Admin assigns TPDs to Schemes (1:1) → Admin assigns EYDs to Schemes → Admin/TPD assigns ES to EYDs (many-to-many)
- **Dean** → Not assigned to Scheme, uses search/temporary access for cross-area visibility

### **Migration Strategy:**
- Current users are already assigned to Areas
- Need to reassign EYDs and TPDs to Schemes during migration
- Keep existing Admin-Area assignments

### **Backward Compatibility:**
- Keep AreaId for Admins
- Add SchemeId for EYDs and TPDs
- Maintain existing dashboard routing

### **Testing Strategy:**
- Test with existing 7 users
- Verify role-based access after changes
- Test search and temporary access features
- **Test Admin Scheme Management:**
  - Create new schemes within area
  - Assign TPDs to schemes (1:1)
  - Assign EYDs to schemes  
  - Assign ES to EYDs (many-to-many)
  - Archive old schemes
- **Test Cross-Area Access:**
  - TPD/Dean search functionality
  - Temporary access requests and approvals

---

**This plan ensures a systematic approach to restructuring the system according to your requirements while maintaining existing functionality.**
