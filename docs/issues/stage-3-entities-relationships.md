# Stage 3: Entities and Relationships

## Objective
Define the domain model with proper entity classes, relationships, and database context configuration.

## Description
Create all domain entities representing the core business objects (Employee, PerformanceReview, Feedback, ReminderLog) with their properties, relationships, and constraints. Configure Entity Framework Core DbContext with proper Fluent API mappings to ensure referential integrity and optimal database schema.

## Entities to Define

### 1. Employee
**Properties:**
- `int Id` (Primary Key)
- `string Name` (Required, max 200 chars)
- `string Email` (Required, max 200 chars, unique)
- `string Department` (Required, max 100 chars)
- `string Role` (max 100 chars)
- `bool IsActive` (default true)
- `DateTime CreatedDate`
- `DateTime? ModifiedDate`

**Relationships:**
- One-to-many with PerformanceReview (as reviewee)
- One-to-many with PerformanceReview (as reviewer)
- One-to-many with Feedback (as provider)

### 2. PerformanceReview
**Properties:**
- `int Id` (Primary Key)
- `int EmployeeId` (Foreign Key - reviewee)
- `int ReviewerId` (Foreign Key - reviewer)
- `DateTime ScheduledDate`
- `string Status` (Required: Pending, InProgress, Completed, Cancelled)
- `string Period` (e.g., "Q1 2024", max 50 chars)
- `string Notes` (optional, max 1000 chars)
- `DateTime CreatedDate`
- `DateTime? CompletedDate`

**Relationships:**
- Many-to-one with Employee (reviewee)
- Many-to-one with Employee (reviewer)
- One-to-many with Feedback
- One-to-many with ReminderLog

**Constraints:**
- EmployeeId cannot equal ReviewerId (self-review not allowed)

### 3. Feedback
**Properties:**
- `int Id` (Primary Key)
- `int PerformanceReviewId` (Foreign Key)
- `int ProviderId` (Foreign Key - feedback provider)
- `string Content` (Required, max 2000 chars)
- `DateTime SubmittedDate`
- `int? Rating` (optional, 1-5 scale)

**Relationships:**
- Many-to-one with PerformanceReview
- Many-to-one with Employee (provider)

**Constraints:**
- One feedback per provider per review (unique constraint on ProviderId + PerformanceReviewId)

### 4. ReminderLog
**Properties:**
- `int Id` (Primary Key)
- `int PerformanceReviewId` (Foreign Key)
- `int RecipientId` (Foreign Key - who receives the reminder)
- `DateTime SentDate`
- `string ReminderType` (e.g., "MissingFeedback", "UpcomingReview")
- `string Message` (max 500 chars)

**Relationships:**
- Many-to-one with PerformanceReview
- Many-to-one with Employee (recipient)

## Tasks
- [ ] Create `Employee.cs` entity class in `Models/` folder
- [ ] Create `PerformanceReview.cs` entity class in `Models/` folder
- [ ] Create `Feedback.cs` entity class in `Models/` folder
- [ ] Create `ReminderLog.cs` entity class in `Models/` folder
- [ ] Create `ApplicationDbContext.cs` in `Data/` folder
- [ ] Configure entity relationships using Fluent API in DbContext
- [ ] Set up cascading delete behaviors appropriately
- [ ] Configure indexes for performance (Email, ScheduledDate, etc.)
- [ ] Add custom validation constraints
- [ ] Add XML documentation comments to all entities
- [ ] Create initial migration
- [ ] (Optional) Add seed data for development/testing

## Acceptance Criteria
- All entity classes have proper properties with data annotations
- DbContext properly configured with DbSet properties for each entity
- Fluent API configurations define all relationships correctly
- Foreign key constraints are properly defined
- Unique constraints are enforced (e.g., Employee.Email)
- Navigation properties are defined for relationships
- Database migration can be created successfully
- Migration applies cleanly to SQLite database
- No circular references or ambiguous relationships
- Code follows C# 14 conventions and naming standards

## Technical Notes
- Use nullable reference types appropriately
- Consider using enums for Status and ReminderType
- Use UTC for all DateTime values
- Configure cascade delete carefully (restrict where appropriate)
- Index frequently queried columns

## Dependencies
- Stage 1 (Solution Structure) must be complete
- Stage 2 (Folder Organization) must be complete

## Estimated Effort
Medium - 2-3 hours
