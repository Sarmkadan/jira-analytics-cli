# DateTimeExtensionsTestsExtensions

Extension methods for `DateTime` that provide business day, holiday, and time-of-day utilities. These methods are designed to simplify date manipulation in business contexts, such as calculating business days, business hours, and standard holidays.

## API

### `public static DateTime AtBusinessStart(DateTime date)`

Returns a `DateTime` representing the start of the business day (typically 9:00 AM) for the given date. If the date is a weekend or holiday, the result is adjusted to the next business day.

- **Parameters**: `date` – The input date to adjust.
- **Returns**: A `DateTime` at the business start time (e.g., 9:00 AM) on the nearest business day.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---

### `public static DateTime AtBusinessEnd(DateTime date)`

Returns a `DateTime` representing the end of the business day (typically 5:00 PM) for the given date. If the date is a weekend or holiday, the result is adjusted to the previous business day.

- **Parameters**: `date` – The input date to adjust.
- **Returns**: A `DateTime` at the business end time (e.g., 5:00 PM) on the nearest business day.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime AtWeekendStart(DateTime date)`

Returns a `DateTime` representing the start of the weekend (typically 12:00 AM on Saturday) for the given date.

- **Parameters**: `date` – The input date to adjust.
- **Returns**: A `DateTime` at midnight on the nearest Saturday.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime AtNextBusinessWeekStart(DateTime date)`

Returns a `DateTime` representing the start of the next business week (typically 9:00 AM on the next Monday) for the given date. If the input date is already a business day, the result is the start of the following business week.

- **Parameters**: `date` – The input date to adjust.
- **Returns**: A `DateTime` at the business start time on the next Monday.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime AtPreviousBusinessWeekEnd(DateTime date)`

Returns a `DateTime` representing the end of the previous business week (typically 5:00 PM on the previous Friday) for the given date. If the input date is already a business day, the result is the end of the preceding business week.

- **Parameters**: `date` – The input date to adjust.
- **Returns**: A `DateTime` at the business end time on the previous Friday.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime NewYearsDay(int year)`

Returns a `DateTime` representing New Year's Day (January 1) for the specified year.

- **Parameters**: `year` – The year for which to calculate the holiday.
- **Returns**: A `DateTime` at midnight on January 1 of the given year.
- **Throws**: `ArgumentOutOfRangeException` if the year is outside the valid `DateTime` range.

---
### `public static DateTime ChristmasDay(int year)`

Returns a `DateTime` representing Christmas Day (December 25) for the specified year.

- **Parameters**: `year` – The year for which to calculate the holiday.
- **Returns**: A `DateTime` at midnight on December 25 of the given year.
- **Throws**: `ArgumentOutOfRangeException` if the year is outside the valid `DateTime` range.

---
### `public static DateTime IndependenceDay(int year)`

Returns a `DateTime` representing U.S. Independence Day (July 4) for the specified year. If July 4 falls on a weekend, the observed holiday is adjusted to the nearest weekday.

- **Parameters**: `year` – The year for which to calculate the holiday.
- **Returns**: A `DateTime` at midnight on July 4 of the given year, adjusted to a weekday if necessary.
- **Throws**: `ArgumentOutOfRangeException` if the year is outside the valid `DateTime` range.

---
### `public static IEnumerable<DateTime> GetBusinessDays(DateTime start, DateTime end)`

Returns an enumerable sequence of business days between two dates, inclusive. Weekends and holidays are excluded.

- **Parameters**:
  - `start` – The start date of the range.
  - `end` – The end date of the range.
- **Returns**: An `IEnumerable<DateTime>` of business days between `start` and `end`.
- **Throws**:
  - `ArgumentOutOfRangeException` if `start` or `end` is outside the valid `DateTime` range.
  - `ArgumentException` if `start` is after `end`.

---
### `public static IEnumerable<DateTime> GetBusinessHours(DateTime start, DateTime end)`

Returns an enumerable sequence of `DateTime` values representing each hour within business hours (typically 9:00 AM to 5:00 PM) for each business day between two dates, inclusive. Weekends and holidays are excluded.

- **Parameters**:
  - `start` – The start date of the range.
  - `end` – The end date of the range.
- **Returns**: An `IEnumerable<DateTime>` of hourly timestamps during business hours on business days between `start` and `end`.
- **Throws**:
  - `ArgumentOutOfRangeException` if `start` or `end` is outside the valid `DateTime` range.
  - `ArgumentException` if `start` is after `end`.

---
### `public static DateTime FirstDayOfMonth(DateTime date)`

Returns a `DateTime` representing the first day of the month for the given date.

- **Parameters**: `date` – The input date.
- **Returns**: A `DateTime` at midnight on the first day of the month of `date`.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime LastDayOfMonth(DateTime date)`

Returns a `DateTime` representing the last day of the month for the given date.

- **Parameters**: `date` – The input date.
- **Returns**: A `DateTime` at midnight on the last day of the month of `date`.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime AtMidnight(DateTime date)`

Returns a `DateTime` representing midnight (00:00:00) on the given date.

- **Parameters**: `date` – The input date.
- **Returns**: A `DateTime` at midnight on `date`.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

---
### `public static DateTime AtNoon(DateTime date)`

Returns a `DateTime` representing noon (12:00:00) on the given date.

- **Parameters**: `date` – The input date.
- **Returns**: A `DateTime` at noon on `date`.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid `DateTime` range.

## Usage
