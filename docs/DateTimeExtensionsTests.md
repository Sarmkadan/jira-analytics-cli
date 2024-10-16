# DateTimeExtensionsTests

Unit tests for the `DateTimeExtensions` class, verifying business day calculations, business hour checks, week number determination, date comparisons, and human-readable duration formatting.

## API

### `GetBusinessDaysBetween_SameDay_ReturnsZeroOrOne`
Verifies that the difference between two identical dates returns either 0 or 1 business day, depending on whether the date is considered a business day.

### `GetBusinessDaysBetween_FullWeek_ExcludesWeekends`
Ensures that the difference between two dates spanning a full week (Monday to Sunday) excludes both weekend days from the count.

### `GetBusinessDaysBetween_WeekendOnly_ReturnsZero`
Confirms that when both dates fall on weekend days, the result is 0 business days.

### `IsBusinessHour_At9AM_ReturnsTrue`
Checks that 9:00 AM is considered within business hours.

### `IsBusinessHour_At16_59_ReturnsTrue`
Verifies that 4:59 PM is still considered within business hours.

### `IsBusinessHour_At17_00_ReturnsFalse`
Ensures that 5:00 PM is not considered within business hours.

### `IsBusinessHour_At8AM_ReturnsFalse`
Confirms that 8:00 AM is not considered within business hours.

### `GetWeekNumber_January1_ReturnsWeek1`
Validates that January 1st of any year returns week number 1.

### `GetWeekNumber_January8_ReturnsWeek2`
Ensures that January 8th of any year returns week number 2.

### `IsPast_YesterdayDate_ReturnsTrue`
Checks that a date representing yesterday is correctly identified as past.

### `IsFuture_TomorrowDate_ReturnsTrue`
Verifies that a date representing tomorrow is correctly identified as future.

### `IsPast_FutureDate_ReturnsFalse`
Confirms that a future date is not identified as past.

### `IsFuture_PastDate_ReturnsFalse`
Ensures that a past date is not identified as future.

### `ToHumanReadableDuration_MultipleDays_ReturnsDaysFormat`
Validates that a duration of multiple days is formatted as "X days".

### `ToHumanReadableDuration_SingleDay_ReturnsSingularForm`
Ensures that a duration of exactly one day is formatted as "1 day".

### `ToHumanReadableDuration_Hours_ReturnsHoursFormat`
Verifies that a duration of multiple hours is formatted as "X hours".

### `ToHumanReadableDuration_SingleHour_ReturnsSingularForm`
Confirms that a duration of exactly one hour is formatted as "1 hour".

### `ToHumanReadableDuration_Minutes_ReturnsMinutesFormat`
Ensures that a duration of multiple minutes is formatted as "X minutes".

### `ToHumanReadableDuration_Seconds_ReturnsSecondsFormat`
Validates that a duration of multiple seconds is formatted as "X seconds".

### `GetLastBusinessDayOfMonth_MonthEndingOnFriday_ReturnsFriday`
Checks that when a month ends on a Friday, the last business day is correctly identified as that Friday.

## Usage
