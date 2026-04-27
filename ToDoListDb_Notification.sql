CREATE TABLE IF NOT EXISTS "Notification" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Type" VARCHAR(255),
    "Text" TEXT,
    "ScheduledAt" TIMESTAMP NOT NULL,
    "IsNotified" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotifiedAt" TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "ToDoUser"("UserId")
);

CREATE INDEX IF NOT EXISTS "IX_Notification_UserId" ON "Notification"("UserId");