CREATE TABLE "ToDoUser"
(
    "UserId"           UUID        NOT NULL,
    "TelegramUserId"   BIGINT      NOT NULL,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "RegisteredAt"     TIMESTAMP   NOT NULL,
    CONSTRAINT "PK_ToDoUser" PRIMARY KEY ("UserId")
);

CREATE TABLE "ToDoList"
(
    "Id"        UUID      NOT NULL,
    "UserId"    UUID      NOT NULL,
    "Name"      VARCHAR(255) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "PK_ToDoList" PRIMARY KEY ("Id")
);

CREATE TABLE "ToDoItem"
(
    "Id"              UUID      NOT NULL,
    "UserId"          UUID      NOT NULL,
    "ListId"          UUID      NULL,
    "Name"            VARCHAR(255) NOT NULL,
    "CreatedAt"       TIMESTAMP NOT NULL,
    "State"           INT       NOT NULL,
    "StateChangedAt"  TIMESTAMP NULL,
    "Deadline"        TIMESTAMP NOT NULL,
    CONSTRAINT "PK_ToDoItem" PRIMARY KEY ("Id")
);

ALTER TABLE "ToDoList"
    ADD CONSTRAINT "FK_ToDoList_ToDoUser_UserId"
        FOREIGN KEY ("UserId")
            REFERENCES "ToDoUser" ("UserId");

ALTER TABLE "ToDoItem"
    ADD CONSTRAINT "FK_ToDoItem_ToDoUser_UserId"
        FOREIGN KEY ("UserId")
            REFERENCES "ToDoUser" ("UserId");

ALTER TABLE "ToDoItem"
    ADD CONSTRAINT "FK_ToDoItem_ToDoList_ListId"
        FOREIGN KEY ("ListId")
            REFERENCES "ToDoList" ("Id")
            ON DELETE SET NULL;

CREATE UNIQUE INDEX "UX_ToDoUser_TelegramUserId" ON "ToDoUser" ("TelegramUserId");

CREATE INDEX "IX_ToDoList_UserId" ON "ToDoList" ("UserId");

CREATE INDEX "IX_ToDoItem_UserId" ON "ToDoItem" ("UserId");

CREATE INDEX "IX_ToDoItem_ListId" ON "ToDoItem" ("ListId");