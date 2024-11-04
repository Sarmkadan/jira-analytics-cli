# JsonFormatter
The `JsonFormatter` type is designed to handle the formatting and validation of JSON data, providing various methods to format JSON strings with or without metadata, filter out unwanted data, and prettify the output for better readability. It also includes a validation method to check the validity of the JSON data and retrieve any errors that may occur during the validation process.

## API
* `public JsonFormatter`: The constructor for the `JsonFormatter` class, used to create a new instance.
* `public string Format`: Formats the JSON data into a string. The purpose of this method is to take the JSON data and return a formatted string representation of it. It does not take any parameters and returns a string. It may throw exceptions if the JSON data is invalid or if there is an error during the formatting process.
* `public string FormatWithMetadata`: Formats the JSON data into a string, including metadata. This method is similar to the `Format` method but also includes metadata in the output. It does not take any parameters and returns a string. It may throw exceptions if the JSON data is invalid or if there is an error during the formatting process.
* `public string FormatFiltered`: Formats the JSON data into a string, filtering out unwanted data. The purpose of this method is to take the JSON data, apply a filter to remove unwanted data, and return a formatted string representation of the filtered data. It does not take any parameters and returns a string. It may throw exceptions if the JSON data is invalid or if there is an error during the formatting process.
* `public string Prettify`: Prettifies the JSON data for better readability. This method takes the JSON data and returns a prettified string representation of it, making it easier to read and understand. It does not take any parameters and returns a string. It may throw exceptions if the JSON data is invalid or if there is an error during the prettification process.
* `public (bool IsValid, string[] Errors) Validate`: Validates the JSON data and returns a tuple containing a boolean indicating whether the data is valid and an array of strings representing any errors that occurred during validation. The purpose of this method is to check the validity of the JSON data and retrieve any errors that may have occurred during the validation process. It does not take any parameters and returns a tuple. It does not throw exceptions.

## Usage
The following examples demonstrate how to use the `JsonFormatter` class:
```csharp
// Example 1: Formatting JSON data
JsonFormatter formatter = new JsonFormatter();
string jsonData = "{\"name\":\"John\",\"age\":30}";
string formattedData = formatter.Format(jsonData);
Console.WriteLine(formattedData);

// Example 2: Validating JSON data
JsonFormatter validator = new JsonFormatter();
string jsonDataToValidate = "{\"name\":\"John\",\"age\":30}";
var (isValid, errors) = validator.Validate();
if (isValid)
{
    Console.WriteLine("JSON data is valid");
}
else
{
    Console.WriteLine("JSON data is invalid:");
    foreach (string error in errors)
    {
        Console.WriteLine(error);
    }
}
```

## Notes
When using the `JsonFormatter` class, it is essential to note that the `Format`, `FormatWithMetadata`, `FormatFiltered`, and `Prettify` methods may throw exceptions if the JSON data is invalid or if there is an error during the formatting or prettification process. The `Validate` method, on the other hand, does not throw exceptions and instead returns a tuple containing a boolean indicating whether the data is valid and an array of strings representing any errors that occurred during validation. Additionally, the `JsonFormatter` class is not thread-safe, and it is recommended to create a new instance for each thread or to synchronize access to a shared instance.
