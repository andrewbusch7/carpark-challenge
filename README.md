# Carpark

This is a simple Azure Functions project to calculate the cost of a carpark session where there are various cost rules at
play.

It demonstrates several effective coding practices:

- Rich Domain Models (decoupling domain behaviour from application integration)
- Testing behaviour not implementation
- Picking the right design pattern for a task (e.g. Gang of Four's Strategy Pattern)

Throughout the code, there are comments where alternate approaches or extension directions are worth highlighting,
particularly if the application were to scale and take real usage.

## Requirements

**The problem:** We need an API that does a rate calculation engine for a carpark
The inputs for this engine are:

1. Car Entry Date and Time
2. Car Exit Date and Time

Based on these 2 inputs the engine program should calculate the correct rate for the customer and display the name of the rate along with the total price to the customer using the following rates:

| Name of the Rate | Early Bird                       |
| :--------------- | :------------------------------- |
| Type             | Flat Rate                        |
| Total Price      | \$13.00                          |
| Entry Condition  | Enter between 6:00 AM to 9:00 AM |
| Exit Condition   | Exit between 3:30 PM to 11:30 PM |

<br />

| Name of the Rate | Night Rate                                   |
| :--------------- | :------------------------------------------- |
| Type             | Flat Rate                                    |
| Total Price      | \$6.50                                       |
| Entry Condition  | Enter between 6:00 PM to midnight (weekdays) |
| Exit Condition   | Exit between 3:30 PM to 11:30 PM             |

<br />

| Name of the Rate | Weekend Rate                                    |
| :--------------- | :---------------------------------------------- |
| Type             | Flat Rate                                       |
| Total Price      | \$10.00                                         |
| Entry Condition  | Enter anytime past midnight on Friday to Sunday |
| Exit Condition   | Exit any time before midnight of Sunday         |

_Note: If a customer enters the carpark before midnight on Friday and if they qualify for Night rate on a Saturday morning, then the program should charge the night rate instead of weekend rate._

For any other entry and exit times the program should refer the following table for calculating the total price.

| Name of the Rate | Weekend Rate                                       |
| :--------------- | :------------------------------------------------- |
| Type             | Standard Rate                                      |
| 0-1 Hours        | \$5.00                                             |
| 1-2 Hours        | \$10.00                                            |
| 2-3 Hours        | \$15.00                                            |
| 3+ Hours         | \$20.00 flat rate for each calendar day of parking |

Note: The customer should get the cheapest deal based on the rules which apply to the time period.
