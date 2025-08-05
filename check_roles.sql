-- Check roles in database
SELECT * FROM "AspNetRoles";

-- Check users and their roles
SELECT u."UserName", u."DisplayName", u."Role", r."Name" as AssignedRole
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
LEFT JOIN "AspNetRoles" r ON ur."RoleId" = r."Id";
