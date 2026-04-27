SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' AND "State" = 0;

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' AND "State" = 0;

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "Id" = 'e5f6a7b8-c9d0-1234-ef01-345678901234';

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "Id" = 'f6a7b8c9-d0e1-2345-f012-456789012345';

SELECT COUNT(*) > 0 AS "Exists"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' AND "Name" = 'Complete project';

SELECT COUNT(*) > 0 AS "Exists"
FROM "ToDoItem"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' AND "Name" = 'Learn C#';

SELECT COUNT(*) AS "ActiveCount"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' AND "State" = 0;

SELECT COUNT(*) AS "ActiveCount"
FROM "ToDoItem"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' AND "State" = 0;

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' 
  AND "Deadline" > '2025-03-01'
  AND "State" = 0;

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' 
  AND "Deadline" > '2025-03-01'
  AND "State" = 0;

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' AND "ListId" = 'c3d4e5f6-a7b8-9012-cdef-123456789012';

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' AND "ListId" IS NULL;

SELECT "Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline"
FROM "ToDoItem"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901' AND "ListId" = 'd4e5f6a7-b8c9-0123-def0-234567890123';

SELECT "UserId", "TelegramUserId", "TelegramUserName", "RegisteredAt"
FROM "ToDoUser"
WHERE "TelegramUserId" = 123456789;

SELECT "UserId", "TelegramUserId", "TelegramUserName", "RegisteredAt"
FROM "ToDoUser"
WHERE "TelegramUserId" = 987654321;

SELECT "Id", "UserId", "Name", "CreatedAt"
FROM "ToDoList"
WHERE "UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';

SELECT "Id", "UserId", "Name", "CreatedAt"
FROM "ToDoList"
WHERE "UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';

SELECT 
    ti."Id",
    ti."UserId",
    ti."ListId",
    ti."Name",
    ti."CreatedAt",
    ti."State",
    ti."StateChangedAt",
    ti."Deadline",
    tu."TelegramUserName",
    tl."Name" AS "ListName"
FROM "ToDoItem" ti
LEFT JOIN "ToDoUser" tu ON ti."UserId" = tu."UserId"
LEFT JOIN "ToDoList" tl ON ti."ListId" = tl."Id"
WHERE ti."UserId" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';

SELECT 
    ti."Id",
    ti."UserId",
    ti."ListId",
    ti."Name",
    ti."CreatedAt",
    ti."State",
    ti."StateChangedAt",
    ti."Deadline",
    tu."TelegramUserName",
    tl."Name" AS "ListName"
FROM "ToDoItem" ti
LEFT JOIN "ToDoUser" tu ON ti."UserId" = tu."UserId"
LEFT JOIN "ToDoList" tl ON ti."ListId" = tl."Id"
WHERE ti."UserId" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';