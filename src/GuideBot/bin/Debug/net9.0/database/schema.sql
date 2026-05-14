CREATE TABLE IF NOT EXISTS "GuideUser"
(
    "Id" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "Role" INT NOT NULL,
    "RegisteredAt" TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_GuideUser_TelegramUserId"
    ON "GuideUser" ("TelegramUserId");

CREATE TABLE IF NOT EXISTS "AttractionCategory"
(
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT NOT NULL,
    "SortOrder" INT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Attraction"
(
    "Id" UUID PRIMARY KEY,
    "CategoryId" UUID NOT NULL REFERENCES "AttractionCategory" ("Id") ON DELETE CASCADE,
    "Name" VARCHAR(255) NOT NULL,
    "ShortDescription" TEXT NOT NULL,
    "FullDescription" TEXT NOT NULL,
    "Address" VARCHAR(255) NOT NULL,
    "PhotoUrl" TEXT NOT NULL,
    "MapUrl" TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Attraction_CategoryId"
    ON "Attraction" ("CategoryId");

CREATE TABLE IF NOT EXISTS "CityInfo"
(
    "Id" INT PRIMARY KEY,
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT NOT NULL,
    "MapUrl" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "AttractionFavorite"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "GuideUser" ("Id") ON DELETE CASCADE,
    "AttractionId" UUID NOT NULL REFERENCES "Attraction" ("Id") ON DELETE CASCADE,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "UX_AttractionFavorite_User_Attraction" UNIQUE ("UserId", "AttractionId")
);

CREATE INDEX IF NOT EXISTS "IX_AttractionFavorite_UserId" ON "AttractionFavorite" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AttractionFavorite_AttractionId" ON "AttractionFavorite" ("AttractionId");

CREATE TABLE IF NOT EXISTS "AttractionLike"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "GuideUser" ("Id") ON DELETE CASCADE,
    "AttractionId" UUID NOT NULL REFERENCES "Attraction" ("Id") ON DELETE CASCADE,
    "IsLike" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "UX_AttractionLike_User_Attraction" UNIQUE ("UserId", "AttractionId")
);

CREATE INDEX IF NOT EXISTS "IX_AttractionLike_UserId" ON "AttractionLike" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AttractionLike_AttractionId" ON "AttractionLike" ("AttractionId");

CREATE TABLE IF NOT EXISTS "AttractionComment"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "GuideUser" ("Id") ON DELETE CASCADE,
    "AttractionId" UUID NOT NULL REFERENCES "Attraction" ("Id") ON DELETE CASCADE,
    "Text" TEXT NOT NULL,
    "IsApproved" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_AttractionComment_UserId" ON "AttractionComment" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AttractionComment_AttractionId" ON "AttractionComment" ("AttractionId");

CREATE TABLE IF NOT EXISTS "Route"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "GuideUser" ("Id") ON DELETE CASCADE,
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "IsThematic" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Route_UserId" ON "Route" ("UserId");

CREATE TABLE IF NOT EXISTS "RouteAttraction"
(
    "Id" UUID PRIMARY KEY,
    "RouteId" UUID NOT NULL REFERENCES "Route" ("Id") ON DELETE CASCADE,
    "AttractionId" UUID NOT NULL REFERENCES "Attraction" ("Id") ON DELETE CASCADE,
    "SortOrder" INT NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_RouteAttraction_RouteId" ON "RouteAttraction" ("RouteId");
CREATE INDEX IF NOT EXISTS "IX_RouteAttraction_AttractionId" ON "RouteAttraction" ("AttractionId");

CREATE TABLE IF NOT EXISTS "Event"
(
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Description" TEXT NOT NULL,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP,
    "Location" VARCHAR(255),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS "Notification"
(
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "GuideUser" ("Id") ON DELETE CASCADE,
    "Message" TEXT NOT NULL,
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Notification_UserId" ON "Notification" ("UserId");
