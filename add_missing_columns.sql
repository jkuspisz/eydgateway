-- Add missing columns to AdHocESReports table
ALTER TABLE "AdHocESReports" 
ADD COLUMN IF NOT EXISTS "ESClinicalPerformance" character varying(2000),
ADD COLUMN IF NOT EXISTS "ESProfessionalBehavior" character varying(2000),
ADD COLUMN IF NOT EXISTS "ESProgressSinceLastReview" character varying(2000),
ADD COLUMN IF NOT EXISTS "ESAdditionalComments" character varying(2000),
ADD COLUMN IF NOT EXISTS "EYDReflectionComments" character varying(2000),
ADD COLUMN IF NOT EXISTS "EYDLearningGoals" character varying(2000),
ADD COLUMN IF NOT EXISTS "EYDActionPlan" character varying(2000);
