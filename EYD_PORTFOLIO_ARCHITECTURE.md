# EYD Portfolio Architecture & Access Control Design

## Overview
This document outlines the architecture for the EYD Portfolio system - the core value proposition of the EYD Gateway Platform. The EYD Portfolio is a personal workspace where Early Years Dentists build their professional portfolio, distinct from the administrative dashboards used by other user types.

## Core Concepts

### EYD Portfolio (Primary Workspace)
- **Personal Portfolio Interface**: Rich workspace for reflections, competencies, evidence, tasks
- **Universal Structure**: Same portfolio components regardless of scheme assignment
- **Content Creation Hub**: Where EYDs input the bulk of their professional development
- **Self-Contained Progress**: Portfolio shows completion status, deadlines, analytics
- **Scheme Reference**: Minor display of scheme assignment for context only

### Access Control Philosophy
- **Scheme Assignment**: Administrative layer for access control and reporting
- **Portfolio Content**: Universal structure independent of scheme
- **Flexible Supervision**: Multiple pathways for supervisors to access portfolios

## User Access Patterns

### EYD Users
- **Own Portfolio Only**: Full read/write access to their personal portfolio
- **No Administrative Views**: Focus purely on portfolio development

### ES (Educational Supervisor) Users
- **Strict Boundaries**: Only assigned EYDs via `EYDESAssignments` table
- **Simple Dashboard**: Shows 1-2 assigned EYDs with direct portfolio links
- **No Extended Access**: Never needs out-of-area EYD access
- **Portfolio Integration**: Works within EYD portfolio for assessment tasks

### TPD (Training Programme Director) Users
- **Default Scheme View**: Portfolio summaries for EYDs in their assigned scheme
- **Area Flexibility**: Dropdown to view other schemes in same area (for panel work)
- **Portfolio Summary Rows**: Aggregated metrics from EYD portfolios (reflections completed, assessments pending)
- **Same Interface**: Consistent UI regardless of scheme selected
- **Area-Restricted**: Cannot access schemes outside their area

### Dean Users
- **Area Overview**: Default view of all schemes in their area
- **Scheme Drill-down**: Can focus on specific schemes like TPDs
- **Cross-Area Search**: Global search for any EYD by name, GDC number, etc.
- **Flexible Access**: Can view any EYD portfolio when needed
- **No Geographic Limits**: Access across all areas for external reviews

### Admin/Superuser
- **Administrative Access**: Based on existing area/system restrictions
- **User Management**: Maintain assignments and system configuration

## UI Design Patterns

### TPD Dashboard Layout
```
┌─────────────────────────────────────────┐
│ Scheme: [GM South ▼] (area schemes)     │
│ ┌─────────────────────────────────────┐ │
│ │ EYD Name    │ Portfolio Summary    │ │
│ │ John Smith  │ 15/20 Reflections   │ │
│ │ Sarah Jones │ Pending Assessment  │ │
│ │ Mike Brown  │ 18/20 Complete      │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

### Dean Dashboard Layout
```
┌─────────────────────────────────────────┐
│ View: [All Area Schemes ▼] Search: [...] │
│ ┌─────────────────────────────────────┐ │
│ │ Multiple scheme summaries OR        │ │
│ │ Search results from any area        │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

### ES Dashboard Layout
```
┌─────────────────────────────────────────┐
│ Your Assigned EYDs                      │
│ ┌─────────────────────────────────────┐ │
│ │ → John Smith Portfolio              │ │
│ │ → Sarah Jones Portfolio             │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## Future Enhancements

### Supervised Learning Event (SLE) Invitations
- **Invitation System**: EYDs can invite any ES/TPD for specific assessments
- **Flexible Supervision**: Not limited to formally assigned supervisors
- **Same-Area Restriction**: Invitations limited to ES/TPD users in same area/region
- **Temporary Access**: Invitation-based access for specific SLE assessment only
- **Multiple Invitations**: EYDs can have multiple pending invitations
- **No Time Limits**: Invitations remain active until completed
- **External Assessors**: Future capability for non-user external assessments

### Portfolio Components (To Be Developed)
- **Reflection Journal**: Structured reflection entries
- **Competency Tracking**: Progress against professional standards
- **Evidence Repository**: Document/file upload and organization
- **Assessment Tasks**: Integration with supervisor assessment workflows
- **Progress Analytics**: Visual progress indicators and completion tracking
- **Learning Objectives**: Goal setting and milestone tracking

## Technical Implementation Notes

### Data Model Considerations
- **Scheme Assignment**: `ApplicationUser.SchemeId` for administrative access control
- **ES Assignments**: `EYDESAssignments` table for formal supervision relationships
- **Portfolio Data**: Universal structure independent of scheme
- **Access Permissions**: Role-based with flexible boundary conditions

### Access Control Logic
1. **ES Access**: Check `EYDESAssignments` for assigned EYDs only
2. **TPD Access**: Scheme-based with area-wide dropdown options
3. **Dean Access**: Area-based default with global search capability
4. **EYD Access**: Own portfolio only via user identity

### Portfolio Summary Generation
- **Real-time Aggregation**: Pull completion metrics from portfolio components
- **Cached Summaries**: Consider performance optimization for large datasets
- **Flexible Metrics**: Configurable summary fields based on portfolio components

## Implementation Priority

1. **Enhanced EYD Portfolio Workspace**: Core portfolio interface with universal components
2. **Portfolio Access Control**: Role-based access with flexible boundaries
3. **TPD Dashboard Enhancement**: Scheme dropdown and portfolio summary rows
4. **Dean Dashboard**: Area overview with global search capability
5. **ES Dashboard Simplification**: Direct assigned EYD portfolio access
6. **SLE Invitation System**: Future phase for flexible assessment access

## Notes
- **Scheme Independence**: Portfolio content is universal regardless of scheme assignment
- **Administrative Layer**: Scheme assignments are for access control and reporting only
- **Flexible Supervision**: System supports both formal assignments and invitation-based access
- **Scalable Design**: Architecture supports future enhancements like external assessors
