INSERT INTO "ToDoUser" ("UserId", "TelegramUserId", "TelegramUserName", "RegisteredAt")
VALUES 
    ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 123456789, 'ivan_petrov', '2025-01-15 10:30:00'),
    ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 987654321, 'maria_sidorova', '2025-02-20 14:45:00');

INSERT INTO "ToDoList" ("Id", "UserId", "Name", "CreatedAt")
VALUES 
    ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'Work Tasks', '2025-01-16 09:00:00'),
    ('d4e5f6a7-b8c9-0123-def0-234567890123', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Personal Tasks', '2025-02-21 08:30:00');

INSERT INTO "ToDoItem" ("Id", "UserId", "ListId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline")
VALUES 
    ('e5f6a7b8-c9d0-1234-ef01-345678901234', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Complete project', '2025-01-17 11:00:00', 0, NULL, '2025-04-01 23:59:59'),
    ('f6a7b8c9-d0e1-2345-f012-456789012345', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', NULL, 'Buy groceries', '2025-01-18 15:30:00', 1, '2025-01-19 10:00:00', '2025-01-20 18:00:00'),

    ('a7b8c9d0-e1f2-3456-0123-567890123456', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'd4e5f6a7-b8c9-0123-def0-234567890123', 'Learn C#', '2025-02-22 09:00:00', 0, NULL, '2025-05-01 23:59:59'),
    ('b8c9d0e1-f2a3-4567-1234-678901234567', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'd4e5f6a7-b8c9-0123-def0-234567890123', 'Read a book', '2025-02-23 20:00:00', 1, '2025-02-28 19:00:00', '2025-03-01 23:59:59');