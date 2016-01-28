using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        public static void Start(List<User> users, List<Product> products)
        {
            PrintWelcomeMessage();

            SetLanguage();

            var user = LoginUser(users);

            if (user != null)
            {
                // Show welcome message
                PrintLoginSuccessfulMessage(user);

                PrintBalance(user);

                // Show product list
                while (true)
                {
                    // Prompt for user input
                    PrintPurchaseMenu(products);

                    int index = ConvertToZeroBasedIndex(ReadNumberWithPrompt(Resources.promptEnterNumber));

                    // Exit if user chose last menu item
                    if (index == products.Count)
                    {
                        SaveData(users, products);

                        PrintExitMessage();
                        return;
                    }
                    else if(index > products.Count)
                    {
                        PrintInvalidSelectionMessage();
                    }
                    else
                    {
                        var product = products[index];
                        PrintConfirmPurchaseMessage(product, user);

                        Console.WriteLine();
                        int quantity = ReadNumberWithPrompt(Resources.promptPurchaseAmount);

                        if (ValidateQuantity(quantity) && 
                            ValidateSufficientQuantityExists(product, quantity) &&
                            ValidateUserHasEnoughMoney(user, product, quantity))
                        {
                            TakeFundsFromUser(user, product, quantity);

                            PrintPurchaseSuccessfulMessage(user, product, quantity);
                        }
                    }
                }
            }

            PrintExitMessage();
        }

        private static void PrintInvalidSelectionMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine(Resources.msgInvalidSelection);
            Console.ResetColor();
        }

        private static void SetLanguage()
        {
            while (true)
            {
                Console.WriteLine("For service in English enter 1");
                Console.WriteLine("Pour le service en français tapez le 2");
                string input = Console.ReadLine();
                if (input == "1")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-CA");
                    return;
                }
                else if (input == "2")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");
                    return;
                }
            }
        }

        private static void PrintPurchaseSuccessfulMessage(User user, Product product, int quantity)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format(Resources.msgPurchased, quantity, product.Name));
            Console.WriteLine(string.Format(Resources.msgNewBalance, user.Balance.ToString("C")));
            Console.ResetColor();
        }

        private static void TakeFundsFromUser(User user, Product product, int quantity)
        {
            user.Balance = user.Balance - product.Price * quantity;
        }

        private static bool ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                PrintInvalidQuantityMessage();
                return false;
            }
            return true;
        }

        private static void PrintInvalidQuantityMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine(Resources.msgInvalidQuantity);
            Console.ResetColor();
        }

        private static bool ValidateSufficientQuantityExists(Product product, int quantity)
        {
            if (product.Quantity < quantity)
            {
                PrintInsufficientStockMessage(product);
                return false;
            }
            return true;
        }

        private static void PrintInsufficientStockMessage(Product product)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            if (product.Quantity == 0)
            {
                Console.WriteLine(string.Format(Resources.msgOutOfStock, product.Name));
            }
            else
            {
                Console.WriteLine(string.Format(Resources.msgInsufficientStock, product.Quantity, product.Name));
            }
            Console.ResetColor();
        }

        private static bool ValidateUserHasEnoughMoney(User user, Product product, int quantity)
        {
            if (user.Balance < product.Price * quantity)
            {
                PrintInsufficientFundsMessage();
                return false;
            }
            return true;
        }

        private static void PrintInsufficientFundsMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(Resources.msgInsufficientFunds);
            Console.ResetColor();
        }

        private static void PrintConfirmPurchaseMessage(Product product, User user)
        {
            Console.WriteLine();
            Console.WriteLine(Resources.msgWantToBuy + product.Name);
            Console.WriteLine(string.Format(Resources.msgBalance, user.Balance.ToString("C")));
        }

        private static void SaveData(List<User> users, List<Product> products)
        {
            SaveUsers(users);
            SaveProducts(products);
        }

        private static void SaveProducts(List<Product> products)
        {
            string json2 = JsonConvert.SerializeObject(products, Formatting.Indented);
            File.WriteAllText(@"Data\Products.json", json2);
        }

        private static void SaveUsers(List<User> users)
        {
            // Write out new balance
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(@"Data\Users.json", json);
        }

        private static int ConvertToZeroBasedIndex(int p)
        {
            return p - 1;
        }

        private static int ReadNumberWithPrompt(string message)
        {
            int? num = null;
            while (num == null)
            {
                // Prompt for user input
                PrintEnterNumberPrompt(message);
                string answer = Console.ReadLine();
                int parsedValue;
                if(Int32.TryParse(answer, out parsedValue))
                {
                    num = parsedValue;
                }
            }
            return num.Value;
        }

        private static void PrintEnterNumberPrompt(string message)
        {
            Console.WriteLine(message);
        }

        private static void PrintPurchaseMenu(List<Product> products)
        {
            Console.WriteLine();
            Console.WriteLine(Resources.promptWhatToBuy);
            for (int i = 0; i < products.Count; i++)
            {
                Product prod = products[i];
                string menuFormat = string.Format("{0}: {1} ({2})", i+1,prod.Name, prod.Price.ToString("C"));
                Console.WriteLine(menuFormat);
            }
            Console.WriteLine(string.Format(Resources.msgMenuExit, products.Count + 1));
        }

        private static void PrintBalance(User user)
        {
            Console.WriteLine();
            Console.WriteLine(string.Format(Resources.msgBalance, user.Balance.ToString("C")));
        }

        private static void PrintLoginSuccessfulMessage(User user)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(string.Format(Resources.msgLoginSuccessful, user.Name));
            Console.ResetColor();
        }

        private static User LoginUser(List<User> users)
        {
            User selectedUser = null;

            while (true)
            {
                PrintUsernamePrompt();
                string name = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }
                selectedUser = users.FirstOrDefault(t => t.Name == name);

                if (selectedUser != null)
                {
                    PrintPasswordPrompt();
                    string pwd = Console.ReadLine();

                    if(ValidatePassword(selectedUser, pwd))
                    {
                        return selectedUser;
                    }
                }
                else
                {
                    PrintInvalidUserMessage();
                }
            }
        }

        private static bool ValidatePassword(User selectedUser, string pwd)
        {
            if(selectedUser.Password == pwd)
            {
                return true;
            }
            else
            {
                PrintInvalidPasswordMessage();
                return false;
            }
        }

        private static void PrintInvalidUserMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(Resources.msgInvalidUser);
            Console.ResetColor();
        }

        private static void PrintInvalidPasswordMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(Resources.msgIncorrectPassword);
            Console.ResetColor();
        }

        private static void PrintPasswordPrompt()
        {
            Console.WriteLine(Resources.msgEnterPassword);
        }

        private static void PrintUsernamePrompt()
        {
            Console.WriteLine();
            Console.WriteLine(Resources.msgEnterUsername);
        }

        private static void PrintExitMessage()
        {
            // Prevent console from closing
            Console.WriteLine();
            Console.WriteLine(Resources.msgExit);
            Console.ReadLine();
        }

        private static void PrintWelcomeMessage()
        {
            // Write welcome message
            Console.WriteLine(Resources.msgWelcome);
            Console.WriteLine("---------------");
        }
    }
}
