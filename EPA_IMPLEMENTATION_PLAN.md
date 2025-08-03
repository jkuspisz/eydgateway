# EPA Implementation Plan

## Overview
This document outlines the implementation plan for the EPA (Entrustable Professional Activities) system within the EYD Gateway platform. The EPA system will provide mandatory competency tracking across specific portfolio activities.

## EPA List (9 Core EPAs)
1. **EPA 1**: Assessing and managing new patients
2. **EPA 2A**: Providing routine dental care: periodontal and restorative
3. **EPA 2B**: Providing routine dental care: removal and replacement of teeth
4. **EPA 3**: Assessing and managing children and young people
5. **EPA 4**: Providing emergency care
6. **EPA 5**: Assessing and managing patients with complex needs
7. **EPA 6**: Promoting oral health in the population
8. **EPA 7**: Managing the service
9. **EPA 8**: Improving the quality of dental services
10. **EPA 9**: Developing self and others

## SLE Types (6 Types)
SLEs (Supervised Learning Events) have 6 distinct types that will be displayed as a list within the SLE portfolio section:

1. **Case Based Discussions**
2. **Direct Observation of Procedural Skills**
3. **Mini-Clinical Evaluation Exercise**
4. **Direct Observation of Procedural Skills Under Simulated Conditions**
5. **Developing the Clinical Teacher**
6. **Direct Evaluation of Non-Technical Learning (DENTL)**

## EPA Mapping Requirements
The following portfolio activities require **mandatory selection of 1-2 EPAs**:
- ✅ Reflection entries
- ✅ SLE forms (all 6 types)
- ✅ Protected Learning Time submissions
- ✅ Significant Event reports
- ✅ Quality Improvement project uploads

## Implementation Phases

### Phase 1: Database Foundation ✅
- [x] Create EPA migration with 9 EPAs pre-populated
- [x] Create EPAMapping table for linking activities to EPAs
- [x] Test database relationships and constraints
- [x] Verify EPA data seeding

### Phase 2: Reusable EPA Components ✅
- [x] EPA Selection Partial View (_EPASelection.cshtml)
- [x] JavaScript validation with 1-2 selection limits
- [x] EPA Service for backend operations
- [x] Integration guide and usage documentation
- [x] Form validation and error handling
- [x] Responsive design and accessibility features

### Phase 3: Reporting & Analytics (Future)
- [ ] EPA progress tracking per EYD
- [ ] ES oversight of EPA completion
- [ ] Portfolio activity EPA mapping reports

## Database Models
- **EPA**: Core EPA definitions with code, title, description
- **EPAMapping**: Links portfolio activities to selected EPAs
- **IEPAMappable**: Interface for portfolio activities that support EPA mapping

## Next Steps
1. Create and run EPA database migration
2. Populate 9 EPAs in database
3. Test EPA models and relationships
4. Plan SLE section implementation

## Notes
- Keep implementation focused and avoid getting ahead on ES/EYD workflows
- SLE section will show list of 6 types, with counts after SLEs are created
- EPA selection is mandatory (1-2 choices) for specific activities only
