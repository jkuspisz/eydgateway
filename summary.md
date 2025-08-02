# EYD Gateway Platform – User Roles, Areas, Schemes, and Functional Logic

---

## Hierarchy & | | Role       | Can See/Manage                                  |
|------------|-------------------------------------------------|
| Superuser  | All Areas, Schemes, Users, Assignments          |
| Admin      | Their Area's Schemes, TPDs, EYDs, ESs           |
| TPD/Dean   | View all schemes in their area + EYDs, cross-area EYD search |
| ES         | Only their assigned EYDs                        |
| EYD        | Only their own dashboard/portfolio              |n   | Their Scheme's EYDs, can view their data (read-only) |ain User Roles

### 1. **Superuser**
- **System-wide control:** Can create, edit, and delete all Areas and Schemes.
- Can create Admin accounts and assign each Admin to a specific Area.
- Can assign Training Programme Directors (TPDs) and Deans to Schemes or Areas as needed.
- Has visibility and management rights for all users and assignments across the system.

### 2. **Admin**
- **Area-specific management:** Can manage Schemes, TPDs, EYDs, and ESs, but only within their assigned Area.
- Cannot modify users or entities outside their Area.

### 3. **Training Programme Director (TPD) / Dean**
- Assigned to a Scheme or Area by Superuser/Admin.
- **New Enhanced View-Only Access:**
  - Can view all schemes within their assigned area (read-only dropdown selection)
  - Can view all EYD users assigned to any scheme within their area
  - Can search for any EYD user across all areas using exact username or GDC number
- **Cannot assign/reassign EYDs** - this functionality is restricted to Admin and Superuser roles only.
- **Security Boundaries:** Area-restricted for scheme viewing, search-only for cross-area access (no browsing).

### 4. **Educational Supervisor (ES)**
- Assigned to one or more EYDs.
- Can only access and interact with the portfolios/records of their assigned EYDs.

### 5. **Early Years Dentist (EYD)**
- Can only access and edit their own dashboard, portfolio, or records.
- Cannot see any other users’ data.

---

## Areas and Schemes – Structure and Logic

### **Area**
- Represents a geographical or organizational grouping (e.g., North West, London, Scotland, etc.).
- Created and managed by Superuser.
- Each Area can have one or more Admins assigned by the Superuser.
- Each Area contains one or more Schemes.

### **Scheme**
- Represents a training cohort or sub-division within an Area (e.g., “Liverpool DFT 2024”, “Manchester DFT 2025”).
- Created by Superuser or the Admin for their assigned Area.
- Each Scheme belongs to one Area (foreign key: `AreaId`).
- Each Scheme has a TPD or Dean (assigned by Superuser/Admin).
- Each Scheme contains many EYDs.
- EYDs are assigned to a Scheme by Admin/Superuser.
- TPDs/Deans can view all EYDs within their Scheme (read-only access).

### **User Assignments**
- **Admins** are assigned to an Area and manage only Schemes and users in that Area.
- **TPDs/Deans** are assigned to one or more Schemes (or an Area); they have enhanced read-only access:
  - View all schemes within their area
  - View EYD users in any scheme within their area
  - Search EYD users across all areas (exact match only)
- **ESs** are assigned to one or more EYDs.
- **EYDs** are assigned to a Scheme.

---

## How It Works

1. **Superuser creates Areas and Schemes** and assigns Admins to Areas.
2. **Admins create and manage Schemes within their Area.**
3. **EYDs are assigned to Schemes** (by Admin or Superuser).
4. **TPDs/Deans are assigned to Schemes** (by Superuser/Admin) and have read-only access to all EYDs in those Schemes.
5. **ESs are assigned to specific EYDs** (by Admin/Superuser only).
6. **Each user’s dashboard and permissions are limited by these assignments and role.**

> No user can view or edit entities outside their assignment (except Superuser, who can see and do everything).

---

## Database Relationships

- `Area` has many `Scheme`s.
- `Scheme` belongs to `Area`; has many `EYD`s; has one or more `TPD/Dean`.
- `Admin` is assigned to an `Area`.
- `TPD/Dean` is assigned to a `Scheme` (or an `Area` if permitted).
- `ES` is assigned to `EYD`(s).
- `EYD` belongs to a `Scheme`.

---

## Example Workflow

- **Superuser** creates Area: “London”.
- Superuser assigns Admin “admin1” to Area “London”.
- Admin “admin1” creates Scheme: “London DFT 2024”.
- Admin assigns TPD “tpd1” to “London DFT 2024”.
- Admin assigns EYDs to “London DFT 2024”.
- Admin assigns ESs to specific EYDs.
- **Each user sees/manages only the entities in their assignment chain.**

---

## Role/Assignment Table

| Role       | Can See/Manage                                  |
|------------|-------------------------------------------------|
| Superuser  | All Areas, Schemes, Users, Assignments          |
| Admin      | Their Area’s Schemes, TPDs, EYDs, ESs           |
| TPD/Dean   | Their Scheme’s EYDs, can view/manage their data |
| ES         | Only their assigned EYDs                        |
| EYD        | Only their own dashboard/portfolio              |

---

## UI Patterns

- Role-based home/dashboard after login.
- Admin/superuser: sidebar or dashboard with management tools.
- TPD/Dean: summary lists and ability to search/request access to other EYDs.
- ES: list/table of assigned EYDs and their progress/portfolios.
- EYD: streamlined single-user dashboard (no sidebar).

---

## Permissions

- Strict role-based access:  
    - Users cannot see or modify outside their scope.
    - Superuser always has full visibility/control.

---

**This summary can be used as a project brief, Copilot context, or included in your README.md.**
