using System;
using System.Collections.Generic;
using System.Linq;

#region Exceptions

/// <summary>
/// Wyjatek rzucany gdy ladunek jest za duzy
/// </summary>
/// <param name="message">Komunikat wyjatku</param>
public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

#endregion

#region Interfaces

/// <summary>
/// Interfejs do wysylania ostrzezen gdy cos nie dziala
/// </summary>
public interface IHazardNotifier
{
    /// <summary>
    /// Wysyla ostrzezenie z podanym komunikatem
    /// </summary>
    /// <param name="message">Wiadomosc</param>
    void NotifyHazard(string message);
}

#endregion

#region Container Classes

/// <summary>
/// Abstrakcyjna klasa dla kontenerow
/// </summary>
public abstract class Container
{
    // Licznik do generowania unikalnych numerow seryjnych
    private static int UniqueSerialNumberCounter = 0;

    /// <summary>
    /// Unikalny numer seryjny kontenera
    /// </summary>
    protected string containerSerialNumber;

    /// <summary>
    /// Aktualna masa ladunku w kilogramach
    /// </summary>
    public int CurrentCargoMassInKilograms { get; protected set; }

    /// <summary>
    /// Masa pustego kontenera w kilogramach
    /// </summary>
    public int EmptyContainerMassInKilograms { get; }

    /// <summary>
    /// Wysokosc kontenera w centymetrach
    /// </summary>
    public int ContainerHeightInCentimeters { get; }

    /// <summary>
    /// Glebokosc kontenera w centymetrach
    /// </summary>
    public int ContainerDepthInCentimeters { get; }

    /// <summary>
    /// Maksymalna ladownosc kontenera w kilogramach
    /// </summary>
    public int MaximumLoadCapacityInKilograms { get; }

    /// <summary>
    /// Numer seryjny kontenera
    /// </summary>
    public string SerialNumber => containerSerialNumber;

    /// <summary>
    /// Konstruktor kontenera
    /// </summary>
    /// <param name="emptyContainerMassInKilograms">Masa pustego kontenera</param>
    /// <param name="containerHeightInCentimeters">Wysokosc kontenera</param>
    /// <param name="containerDepthInCentimeters">Glebokosc kontenera</param>
    /// <param name="maximumLoadCapacityInKilograms">Maksymalna ladownosc</param>
    public Container(int emptyContainerMassInKilograms, int containerHeightInCentimeters, int containerDepthInCentimeters, int maximumLoadCapacityInKilograms)
    {
        EmptyContainerMassInKilograms = emptyContainerMassInKilograms;
        ContainerHeightInCentimeters = containerHeightInCentimeters;
        ContainerDepthInCentimeters = containerDepthInCentimeters;
        MaximumLoadCapacityInKilograms = maximumLoadCapacityInKilograms;
        CurrentCargoMassInKilograms = 0;
        UniqueSerialNumberCounter++;
    }

    /// <summary>
    /// Generuje numer seryjny w formacie KON-C-1
    /// </summary>
    /// <param name="containerType">Kod typu kontenera</param>
    /// <returns>Numer seryjny jako lancuch znakow</returns>
    protected string GenerateSerialNumber(string containerType)
    {
        return $"KON-{containerType}-{UniqueSerialNumberCounter}";
    }

    /// <summary>
    /// Laduje ladunek do kontenera
    /// </summary>
    /// <param name="massInKilograms">Masa ladunku w kilogramach</param>
    public abstract void LoadCargo(int massInKilograms);

    /// <summary>
    /// Oproznia ladunek z kontenera
    /// </summary>
    public abstract void EmptyCargo();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Kontener {containerSerialNumber}\n" +
               $"Typ {GetType().Name}\n" +
               $"Masa ladunku {CurrentCargoMassInKilograms} kg\n" +
               $"Masa pustego kontenera {EmptyContainerMassInKilograms} kg\n" +
               $"Wysokosc {ContainerHeightInCentimeters} cm\n" +
               $"Glebokosc {ContainerDepthInCentimeters} cm\n" +
               $"Maksymalna ladownosc {MaximumLoadCapacityInKilograms} kg";
    }
}

/// <summary>
/// Kontener na plyny
/// moze przewozic ladunek niebezpieczny lub zwykly
/// </summary>
public class LiquidContainer : Container, IHazardNotifier
{
    /// <summary>
    /// Informacja czy ladunek jest niebezpieczny
    /// </summary>
    public bool IsCargoDangerous { get; private set; }

    /// <summary>  
    /// Konstruktor kontenera na plyny
    /// </summary>
    /// <param name="emptyContainerMassInKilograms">Masa pustego kontenera</param>
    /// <param name="containerHeightInCentimeters">Wysokosc kontenera</param>
    /// <param name="containerDepthInCentimeters">Glebokosc kontenera</param>
    /// <param name="maximumLoadCapacityInKilograms">Maksymalna ladownosc</param>
    /// <param name="isCargoDangerous">Czy ladunek jest niebezpieczny</param>
    public LiquidContainer(int emptyContainerMassInKilograms, int containerHeightInCentimeters, int containerDepthInCentimeters, int maximumLoadCapacityInKilograms, bool isCargoDangerous)
        : base(emptyContainerMassInKilograms, containerHeightInCentimeters, containerDepthInCentimeters, maximumLoadCapacityInKilograms)
    {
        IsCargoDangerous = isCargoDangerous;
        containerSerialNumber = GenerateSerialNumber("L");
    }

    /// <inheritdoc/>
    public override void LoadCargo(int massInKilograms)
    {
        int allowedLoadMass = IsCargoDangerous ? (int)(MaximumLoadCapacityInKilograms * 0.5) : (int)(MaximumLoadCapacityInKilograms * 0.9);
        if (massInKilograms > allowedLoadMass)
        {
            NotifyHazard($"Proba zaladowania {massInKilograms} kg przekracza dozwolona wartosc {allowedLoadMass} kg!");
            throw new OverfillException("Przeladowanie kontenera plynowego!");
        }
        CurrentCargoMassInKilograms = massInKilograms;
    }

    /// <inheritdoc/>
    public override void EmptyCargo()
    {
        CurrentCargoMassInKilograms = 0;
    }

    /// <inheritdoc/>
    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Informacja - Kontener {containerSerialNumber} {message}");
    }
}

/// <summary>
/// Kontener gazowy ktory przechowuje gaz
/// przy oproznianiu zostawia 5 proc ladunku
/// </summary>
public class GasContainer : Container, IHazardNotifier
{
    /// <summary>
    /// Cisnienie gazu w atmosferach
    /// </summary>
    public double PressureInAtmospheres { get; private set; }

    /// <summary>
    /// Konstruktor kontenera gazowego
    /// </summary>
    /// <param name="emptyContainerMassInKilograms">Masa pustego kontenera</param>
    /// <param name="containerHeightInCentimeters">Wysokosc kontenera</param>
    /// <param name="containerDepthInCentimeters">Glebokosc kontenera</param>
    /// <param name="maximumLoadCapacityInKilograms">Maksymalna ladownosc</param>
    /// <param name="pressureInAtmospheres">Cisnienie w atmosferach</param>
    public GasContainer(int emptyContainerMassInKilograms, int containerHeightInCentimeters, int containerDepthInCentimeters, int maximumLoadCapacityInKilograms, double pressureInAtmospheres)
        : base(emptyContainerMassInKilograms, containerHeightInCentimeters, containerDepthInCentimeters, maximumLoadCapacityInKilograms)
    {
        PressureInAtmospheres = pressureInAtmospheres;
        containerSerialNumber = GenerateSerialNumber("G");
    }

    /// <inheritdoc/>
    public override void LoadCargo(int massInKilograms)
    {
        if (massInKilograms > MaximumLoadCapacityInKilograms)
        {
            NotifyHazard($"Proba zaladowania {massInKilograms} kg przekracza maksymalna ladownosc {MaximumLoadCapacityInKilograms} kg!");
            throw new OverfillException("Przeladowanie kontenera gazowego!");
        }
        CurrentCargoMassInKilograms = massInKilograms;
    }

    /// <inheritdoc/>
    public override void EmptyCargo()
    {
        CurrentCargoMassInKilograms = (int)(CurrentCargoMassInKilograms * 0.05);
    }

    /// <inheritdoc/>
    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Informacja - Container {containerSerialNumber} {message}");
    }
}

/// <summary>
/// Kontener chlodniczy
/// utrzymuje temperature dla produktu
/// </summary>
public class CoolerContainer : Container
{
    /// <summary>
    /// Typ produktu przechowywanego w kontenerze
    /// </summary>
    public string StoredProductType { get; private set; }

    /// <summary>
    /// Utrzymywana temperatura w stopniach Celsjusza
    /// </summary>
    public int MaintainedTemperatureInCelsius { get; private set; }

    /// <summary>
    /// Minimalna wymagana temperatura w stopniach Celsjusza
    /// </summary>
    public int MinimumRequiredTemperatureInCelsius { get; private set; }

    /// <summary>
    /// Konstruktor kontenera chlodniczego
    /// </summary>
    /// <param name="emptyContainerMassInKilograms">Masa pustego kontenera</param>
    /// <param name="containerHeightInCentimeters">Wysokosc kontenera</param>
    /// <param name="containerDepthInCentimeters">Glebokosc kontenera</param>
    /// <param name="maximumLoadCapacityInKilograms">Maksymalna ladownosc</param>
    /// <param name="storedProductType">Typ produktu</param>
    /// <param name="maintainedTemperatureInCelsius">Utrzymywana temperatura</param>
    /// <param name="minimumRequiredTemperatureInCelsius">Minimalna wymagana temperatura</param>
    public CoolerContainer(int emptyContainerMassInKilograms, int containerHeightInCentimeters,
        int containerDepthInCentimeters, int maximumLoadCapacityInKilograms,
        string storedProductType, int maintainedTemperatureInCelsius, int minimumRequiredTemperatureInCelsius)
        : base(emptyContainerMassInKilograms, containerHeightInCentimeters, containerDepthInCentimeters,
            maximumLoadCapacityInKilograms)
    {
        StoredProductType = storedProductType;
        MinimumRequiredTemperatureInCelsius = minimumRequiredTemperatureInCelsius;
        if (maintainedTemperatureInCelsius < MinimumRequiredTemperatureInCelsius)
        {
            throw new ArgumentException("Utrzymywana temperatura nie moze byc nizsza niz wymagana!");
        }

        MaintainedTemperatureInCelsius = maintainedTemperatureInCelsius;
        containerSerialNumber = GenerateSerialNumber("C");
    }

    /// <inheritdoc/>
    public override void LoadCargo(int massInKilograms)
    {
        if (massInKilograms > MaximumLoadCapacityInKilograms)
        {
            throw new OverfillException("Przeladowanie kontenera chlodniczego!");
        }

        CurrentCargoMassInKilograms = massInKilograms;
    }

    /// <inheritdoc/>
    public override void EmptyCargo()
    {
        CurrentCargoMassInKilograms = 0;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return base.ToString() +
               $"\nTyp produktu {StoredProductType}\nUtrzymywana temperatura {MaintainedTemperatureInCelsius} °C\nMinimalna wymagana temperatura {MinimumRequiredTemperatureInCelsius} °C";
    }

    #endregion

    #region Ship Class

    /// <summary>
    /// Klasa reprezentujaca statek ktory przewozi kontenery
    /// </summary>
    public class Ship
    {
        /// <summary>
        /// Lista kontenerow na statku
        /// </summary>
        public List<Container> ListOfContainersOnShip { get; private set; }

        /// <summary>
        /// Maksymalna liczba kontenerow jakie statek moze przewiesc
        /// </summary>
        public int MaximumNumberOfContainersAllowed { get; }

        /// <summary>
        /// Maksymalna waga kontenerow w tonach ktora statek moze przewiesc
        /// </summary>
        public int MaximumTotalWeightInTons { get; }

        /// <summary>
        /// Maksymalna predkosc statku w wezlach
        /// </summary>
        public int MaximumSpeedInKnots { get; }

        /// <summary>
        /// Konstruktor statku ustawiajacy jego parametry
        /// </summary>
        /// <param name="maximumNumberOfContainersAllowed">Maksymalna liczba kontenerow</param>
        /// <param name="maximumTotalWeightInTons">Maksymalna waga w tonach</param>
        /// <param name="maximumSpeedInKnots">Maksymalna predkosc w wezlach</param>
        public Ship(int maximumNumberOfContainersAllowed, int maximumTotalWeightInTons, int maximumSpeedInKnots)
        {
            ListOfContainersOnShip = new List<Container>();
            MaximumNumberOfContainersAllowed = maximumNumberOfContainersAllowed;
            MaximumTotalWeightInTons = maximumTotalWeightInTons;
            MaximumSpeedInKnots = maximumSpeedInKnots;
        }

        /// <summary>
        /// Dodaje kontener do statku
        /// </summary>
        /// <param name="containerToAdd">Kontener do dodania</param>
        public void AddContainer(Container containerToAdd)
        {
            if (ListOfContainersOnShip.Count >= MaximumNumberOfContainersAllowed)
            {
                throw new Exception("Nie mozna dodac kontenera! osiagnieto limit");
            }

            int totalWeightInKg = GetTotalWeightInKilograms();
            if (totalWeightInKg + (containerToAdd.CurrentCargoMassInKilograms +
                                   containerToAdd.EmptyContainerMassInKilograms) > MaximumTotalWeightInTons * 1000)
            {
                throw new Exception("Nie mozna dodac kontenera! przekroczono wage");
            }

            ListOfContainersOnShip.Add(containerToAdd);
        }

        /// <summary>
        /// Usuwa kontener ze statku na podstawie numeru seryjnego
        /// </summary>
        /// <param name="serialNumberToRemove">Numer seryjny kontenera do usuniecia</param>
        public void RemoveContainer(string serialNumberToRemove)
        {
            Container containerToRemove = ListOfContainersOnShip.Find(c => c.SerialNumber == serialNumberToRemove);
            if (containerToRemove != null)
            {
                ListOfContainersOnShip.Remove(containerToRemove);
            }
        }

        /// <summary>
        /// Zastepuje kontener na statku nowym kontenerem
        /// </summary>
        /// <param name="serialNumberToReplace">Numer seryjny kontenera ktory bedzie zastapiony</param>
        /// <param name="newContainer">Nowy kontener</param>
        public void ReplaceContainer(string serialNumberToReplace, Container newContainer)
        {
            int index = ListOfContainersOnShip.FindIndex(c => c.SerialNumber == serialNumberToReplace);
            if (index >= 0)
            {
                ListOfContainersOnShip[index] = newContainer;
            }
        }

        /// <summary>
        /// Oblicza cala wage kontenerow na statku w kilogramach
        /// </summary>
        /// <returns>Waga w kilogramach</returns>
        public int GetTotalWeightInKilograms()
        {
            int totalWeight = 0;
            foreach (var container in ListOfContainersOnShip)
            {
                totalWeight += container.CurrentCargoMassInKilograms + container.EmptyContainerMassInKilograms;
            }

            return totalWeight;
        }

        /// <summary>
        /// Wyswietla informacje o statku i jego ladunku
        /// </summary>
        /// <param name="shipDisplayIndex">Indeks statku do wyswietlenia</param>
        public void PrintShipInfo(int shipDisplayIndex)
        {
            Console.WriteLine(
                $"Statek {shipDisplayIndex} Predkosc = {MaximumSpeedInKnots} wezlow, Maksymalna liczba kontenerow = {MaximumNumberOfContainersAllowed}, Maksymalna waga = {MaximumTotalWeightInTons * 1000} kg");
            Console.WriteLine("Kontenery na statku");
            if (ListOfContainersOnShip.Count == 0)
                Console.WriteLine("Brak");
            else
            {
                foreach (var container in ListOfContainersOnShip)
                {
                    Console.WriteLine("......................");
                    Console.WriteLine(container.ToString());
                }
            }
        }
    }

    #endregion

    #region Main Program

    /// <summary>
    /// Glowna klasa programu do zarzadzania ladunkiem kontenerow
    /// </summary>
    public class Program
    {
        // Lista statkow i wolnych kontenerow
        private static List<Ship> shipCollection = new List<Ship>();
        private static List<Container> freeContainerCollection = new List<Container>();

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Args</param>
        public static void Main(string[] args)
        {
            bool exitProgram = false;
            while (!exitProgram)
            {
                ShowMainScreen();
                Console.Write("Wybierz opcje: ");
                string selectedUserOption = Console.ReadLine();
                Console.WriteLine();
                switch (selectedUserOption)
                {
                    case "1":
                        AddShip();
                        break;
                    case "2":
                        RemoveShip();
                        break;
                    case "3":
                        AddContainer();
                        break;
                    case "4":
                        LoadCargoForContainer();
                        break;
                    case "5":
                        PlaceContainerOnShip();
                        break;
                    case "6":
                        PlaceMultipleContainersOnShip();
                        break;
                    case "7":
                        RemoveContainerFromShip();
                        break;
                    case "8":
                        UnloadContainer();
                        break;
                    case "9":
                        ReplaceContainerOnShip();
                        break;
                    case "10":
                        TransferContainerBetweenShips();
                        break;
                    case "11":
                        PrintContainerInfo();
                        break;
                    case "12":
                        PrintShipInfo();
                        break;
                    case "0":
                        exitProgram = true;
                        break;
                    default:
                        Console.WriteLine("Niepoprawny wybor. Spróbuj ponownie");
                        break;
                }

                Console.WriteLine("\nNacisnij klawisz enter aby kontynuowac");
                Console.ReadLine();
                Console.Clear();
            }
        }

        /// <summary>
        /// Wyswietla glowny ekran z lista statkow i wolnych kontenerow
        /// </summary>
        private static void ShowMainScreen()
        {
            Console.WriteLine("=== System Zarzadzania Zaladunkiem Kontenerow ===\n");

            Console.WriteLine("Lista statkow:");
            if (shipCollection.Count == 0)
                Console.WriteLine("Brak statkow");
            else
            {
                for (int i = 0; i < shipCollection.Count; i++)
                {
                    Console.WriteLine(
                        $"Statek {i + 1} (Predkosc = {shipCollection[i].MaximumSpeedInKnots} wezlow, Maksymalna liczba kontenerow = {shipCollection[i].MaximumNumberOfContainersAllowed}, Maksymalna waga = {shipCollection[i].MaximumTotalWeightInTons * 1000} kg)");
                }
            }

            Console.WriteLine("\nLista wolnych kontenerow:");
            if (freeContainerCollection.Count == 0)
                Console.WriteLine("Brak wolnych kontenerow");
            else
            {
                foreach (var container in freeContainerCollection)
                {
                    Console.WriteLine(container.ToString());
                    Console.WriteLine("......................");
                }
            }

            Console.WriteLine("\nDostepne opcje:");
            Console.WriteLine("1 Dodaj statek");
            Console.WriteLine("2 Usun statek");
            Console.WriteLine("3 Stworz kontener");
            Console.WriteLine("4 Zaladuj ladunek do kontenera");
            Console.WriteLine("5 Umiesc kontener na statku");
            Console.WriteLine("6 Umiesc liste kontenerow na statku");
            Console.WriteLine("7 Usun kontener ze statku");
            Console.WriteLine("8 Rozladuj kontener");
            Console.WriteLine("9 Zastepuj kontener na statku");
            Console.WriteLine("10 Przenies kontener miedzy statkami");
            Console.WriteLine("11 Wyswietl informacje o kontenerze");
            Console.WriteLine("12 Wyswietl informacje o statku i ladunku");
            Console.WriteLine("0 Wyjscie\n");
        }

        /// <summary>
        /// Dodaje nowy statek na podstawie danych podanych przez uzytkownika
        /// </summary>
        private static void AddShip()
        {
            Console.WriteLine("Dodawanie statku");
            Console.Write("Podaj maksymalna liczbe kontenerow: ");
            int maximumNumberOfContainers = int.Parse(Console.ReadLine());
            Console.Write("Podaj maksymalna wage ladunku w tonach: ");
            int maximumTotalCargoWeightInTons = int.Parse(Console.ReadLine());
            Console.Write("Podaj maksymalna predkosc w wezlach: ");
            int maximumSpeedInKnots = int.Parse(Console.ReadLine());

            Ship newShip = new Ship(maximumNumberOfContainers, maximumTotalCargoWeightInTons, maximumSpeedInKnots);
            shipCollection.Add(newShip);
            Console.WriteLine("Statek zostal dodany");
        }

        /// <summary>
        /// Usuwa wybrany statek i zwraca jego kontenery do listy wolnych
        /// </summary>
        private static void RemoveShip()
        {
            if (shipCollection.Count == 0)
            {
                Console.WriteLine("Brak statkow do usuniecia");
                return;
            }

            Console.WriteLine("Usuwanie statku");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            Console.Write("Wybierz numer statku do usuniecia: ");
            int selectedShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedShipIndex >= 0 && selectedShipIndex < shipCollection.Count)
            {
                foreach (var container in shipCollection[selectedShipIndex].ListOfContainersOnShip)
                {
                    freeContainerCollection.Add(container);
                }

                shipCollection.RemoveAt(selectedShipIndex);
                Console.WriteLine("Statek zostal usuniety");
            }
            else
            {
                Console.WriteLine("Niepoprawny numer statku");
            }
        }

        /// <summary>
        /// Dodaje nowy kontener na podstawie danych podanych przez uzytkownika
        /// </summary>
        private static void AddContainer()
        {
            Console.WriteLine("Tworzenie kontenera");
            Console.WriteLine("Wybierz typ kontenera");
            Console.WriteLine("1 Plynny L");
            Console.WriteLine("2 Gazowy G");
            Console.WriteLine("3 Chlodniczy C");
            Console.Write("Twoj wybor: ");
            string containerTypeChoice = Console.ReadLine();

            Console.Write("Podaj mase pustego kontenera w kg: ");
            int emptyContainerMassInKilograms = int.Parse(Console.ReadLine());
            Console.Write("Podaj wysokosc kontenera w cm: ");
            int containerHeightInCentimeters = int.Parse(Console.ReadLine());
            Console.Write("Podaj glebokosc kontenera w cm: ");
            int containerDepthInCentimeters = int.Parse(Console.ReadLine());
            Console.Write("Podaj maksymalna ladownosc w kg: ");
            int maximumLoadCapacityInKilograms = int.Parse(Console.ReadLine());

            Container newContainer = null;
            try
            {
                if (containerTypeChoice == "1")
                {
                    Console.Write("Czy ladunek jest niebezpieczny (T/N): ");
                    string dangerousChoice = Console.ReadLine();
                    bool isCargoDangerous = dangerousChoice.ToUpper() == "T";
                    newContainer = new LiquidContainer(emptyContainerMassInKilograms, containerHeightInCentimeters,
                        containerDepthInCentimeters, maximumLoadCapacityInKilograms, isCargoDangerous);
                }
                else if (containerTypeChoice == "2")
                {
                    Console.Write("Podaj cisnienie w atmosferach: ");
                    double pressureInAtmospheres = double.Parse(Console.ReadLine());
                    newContainer = new GasContainer(emptyContainerMassInKilograms, containerHeightInCentimeters,
                        containerDepthInCentimeters, maximumLoadCapacityInKilograms, pressureInAtmospheres);
                }
                else if (containerTypeChoice == "3")
                {
                    Console.Write("Podaj rodzaj produktu: ");
                    string productType = Console.ReadLine();
                    Console.Write("Podaj utrzymywana temperature w C: ");
                    int maintainedTemperatureInCelsius = int.Parse(Console.ReadLine());
                    Console.Write("Podaj minimalna wymagana temperature w C: ");
                    int minimumRequiredTemperatureInCelsius = int.Parse(Console.ReadLine());
                    newContainer = new CoolerContainer(emptyContainerMassInKilograms, containerHeightInCentimeters,
                        containerDepthInCentimeters, maximumLoadCapacityInKilograms, productType,
                        maintainedTemperatureInCelsius, minimumRequiredTemperatureInCelsius);
                }
                else
                {
                    Console.WriteLine("Niepoprawny wybor typu kontenera");
                    return;
                }

                freeContainerCollection.Add(newContainer);
                Console.WriteLine($"Kontener {newContainer.SerialNumber} zostal dodany do listy wolnych kontenerow");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blad przy dodawaniu kontenera: {ex.Message}");
            }
        }

        /// <summary>
        /// Laduje ladunek do wybranego kontenera
        /// </summary>
        private static void LoadCargoForContainer()
        {
            List<Container> allContainers = new List<Container>();
            allContainers.AddRange(freeContainerCollection);
            foreach (var ship in shipCollection)
                allContainers.AddRange(ship.ListOfContainersOnShip);

            if (allContainers.Count == 0)
            {
                Console.WriteLine("Brak kontenerow do zaladowania");
                return;
            }

            Console.WriteLine("Wybierz kontener do zaladowania ladunku");
            for (int i = 0; i < allContainers.Count; i++)
            {
                Console.WriteLine($"{i + 1} {allContainers[i].SerialNumber}");
            }

            int selectedContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedContainerIndex < 0 || selectedContainerIndex >= allContainers.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            Console.Write("Podaj mase ladunku do zaladowania w kg: ");
            int loadMassInKilograms = int.Parse(Console.ReadLine());

            try
            {
                allContainers[selectedContainerIndex].LoadCargo(loadMassInKilograms);
                Console.WriteLine(
                    $"Ladunek zostal zaladowany do kontenera {allContainers[selectedContainerIndex].SerialNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blad przy zaladowaniu ladunku: {ex.Message}");
            }
        }

        /// <summary>
        /// Umieszcza jeden wolny kontener na wybranym statku
        /// </summary>
        private static void PlaceContainerOnShip()
        {
            if (freeContainerCollection.Count == 0)
            {
                Console.WriteLine("Brak wolnych kontenerow do umieszczenia");
                return;
            }

            Console.WriteLine("Wybierz wolny kontener z listy");
            for (int i = 0; i < freeContainerCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} {freeContainerCollection[i].SerialNumber}");
            }

            int selectedFreeContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedFreeContainerIndex < 0 || selectedFreeContainerIndex >= freeContainerCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            if (shipCollection.Count == 0)
            {
                Console.WriteLine("Brak statkow; dodaj najpierw statek");
                return;
            }

            Console.WriteLine("Wybierz statek");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int selectedShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedShipIndex < 0 || selectedShipIndex >= shipCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor statku");
                return;
            }

            try
            {
                shipCollection[selectedShipIndex].AddContainer(freeContainerCollection[selectedFreeContainerIndex]);
                Console.WriteLine(
                    $"Kontener {freeContainerCollection[selectedFreeContainerIndex].SerialNumber} zostal umieszczony na statku {selectedShipIndex + 1}");
                freeContainerCollection.RemoveAt(selectedFreeContainerIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blad przy umieszczaniu kontenera na statku: {ex.Message}");
            }
        }

        /// <summary>
        /// Umieszcza wiele wolnych kontenerow na wybranym statku
        /// </summary>
        private static void PlaceMultipleContainersOnShip()
        {
            if (freeContainerCollection.Count == 0)
            {
                Console.WriteLine("Brak wolnych kontenerow do umieszczenia");
                return;
            }

            Console.WriteLine("Dostepne wolne kontenery");
            for (int i = 0; i < freeContainerCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} {freeContainerCollection[i].SerialNumber}");
            }

            Console.Write("Podaj numery kontenerow do umieszczenia oddzielone przecinkami: ");
            string inputIndices = Console.ReadLine();
            var selectedIndices = inputIndices.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim()) - 1).ToList();

            if (shipCollection.Count == 0)
            {
                Console.WriteLine("Brak statkow; dodaj najpierw statek");
                return;
            }

            Console.WriteLine("Wybierz statek na ktory chcesz umiescic kontenery");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int selectedShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedShipIndex < 0 || selectedShipIndex >= shipCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor statku");
                return;
            }

            selectedIndices = selectedIndices.OrderByDescending(i => i).ToList();
            foreach (int index in selectedIndices)
            {
                if (index >= 0 && index < freeContainerCollection.Count)
                {
                    try
                    {
                        shipCollection[selectedShipIndex].AddContainer(freeContainerCollection[index]);
                        Console.WriteLine(
                            $"Kontener {freeContainerCollection[index].SerialNumber} zostal umieszczony na statku {selectedShipIndex + 1}");
                        freeContainerCollection.RemoveAt(index);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Blad przy dodawaniu kontenera {freeContainerCollection[index].SerialNumber}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Niepoprawny indeks {index + 1}");
                }
            }
        }

        /// <summary>
        /// Usuwa kontener ze statku i dodaje go do listy wolnych kontenerow
        /// </summary>
        private static void RemoveContainerFromShip()
        {
            if (shipCollection.Count == 0)
            {
                Console.WriteLine("Brak statkow");
                return;
            }

            Console.WriteLine("Wybierz statek");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int selectedShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedShipIndex < 0 || selectedShipIndex >= shipCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor statku");
                return;
            }

            if (shipCollection[selectedShipIndex].ListOfContainersOnShip.Count == 0)
            {
                Console.WriteLine("Na tym statku nie ma kontenerow");
                return;
            }

            Console.WriteLine("Wybierz kontener do usuniecia");
            for (int i = 0; i < shipCollection[selectedShipIndex].ListOfContainersOnShip.Count; i++)
            {
                Console.WriteLine(
                    $"{i + 1} {shipCollection[selectedShipIndex].ListOfContainersOnShip[i].SerialNumber}");
            }

            int selectedContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedContainerIndex < 0 ||
                selectedContainerIndex >= shipCollection[selectedShipIndex].ListOfContainersOnShip.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            Container containerToRemove =
                shipCollection[selectedShipIndex].ListOfContainersOnShip[selectedContainerIndex];
            shipCollection[selectedShipIndex].RemoveContainer(containerToRemove.SerialNumber);
            freeContainerCollection.Add(containerToRemove);
            Console.WriteLine(
                $"Kontener {containerToRemove.SerialNumber} zostal usuniety ze statku i dodany do listy wolnych kontenerow");
        }

        /// <summary>
        /// Oproznia wybrany kontener z ladunku
        /// </summary>
        private static void UnloadContainer()
        {
            List<Container> allContainers = new List<Container>();
            allContainers.AddRange(freeContainerCollection);
            foreach (var ship in shipCollection)
                allContainers.AddRange(ship.ListOfContainersOnShip);

            if (allContainers.Count == 0)
            {
                Console.WriteLine("Brak kontenerow");
                return;
            }

            Console.WriteLine("Wybierz kontener do rozladowania");
            for (int i = 0; i < allContainers.Count; i++)
            {
                Console.WriteLine($"{i + 1} {allContainers[i].SerialNumber}");
            }

            int selectedContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedContainerIndex < 0 || selectedContainerIndex >= allContainers.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            allContainers[selectedContainerIndex].EmptyCargo();
            Console.WriteLine($"Kontener {allContainers[selectedContainerIndex].SerialNumber} zostal rozladowany");
        }

        /// <summary>
        /// Zastepuje kontener na statku nowym kontenerem z listy wolnych
        /// </summary>
        private static void ReplaceContainerOnShip()
        {
            if (shipCollection.Count == 0)
            {
                Console.WriteLine("Brak statkow");
                return;
            }

            Console.WriteLine("Wybierz statek");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int selectedShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedShipIndex < 0 || selectedShipIndex >= shipCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor statku");
                return;
            }

            if (shipCollection[selectedShipIndex].ListOfContainersOnShip.Count == 0)
            {
                Console.WriteLine("Na tym statku nie ma kontenerow do zastapienia");
                return;
            }

            Console.WriteLine("Wybierz kontener na statku do zastapienia");
            for (int i = 0; i < shipCollection[selectedShipIndex].ListOfContainersOnShip.Count; i++)
            {
                Console.WriteLine(
                    $"{i + 1} {shipCollection[selectedShipIndex].ListOfContainersOnShip[i].SerialNumber}");
            }

            int selectedContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedContainerIndex < 0 ||
                selectedContainerIndex >= shipCollection[selectedShipIndex].ListOfContainersOnShip.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            if (freeContainerCollection.Count == 0)
            {
                Console.WriteLine("Brak wolnych kontenerow do zastapienia");
                return;
            }

            Console.WriteLine("Wybierz wolny kontener ktory zastapi wybrany kontener");
            for (int i = 0; i < freeContainerCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} {freeContainerCollection[i].SerialNumber}");
            }

            int selectedFreeContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedFreeContainerIndex < 0 || selectedFreeContainerIndex >= freeContainerCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            string replacedContainerSerial = shipCollection[selectedShipIndex]
                .ListOfContainersOnShip[selectedContainerIndex].SerialNumber;
            shipCollection[selectedShipIndex].ReplaceContainer(replacedContainerSerial,
                freeContainerCollection[selectedFreeContainerIndex]);
            freeContainerCollection.RemoveAt(selectedFreeContainerIndex);
            Console.WriteLine($"Kontener {replacedContainerSerial} zostal zastapiony nowym kontenerem");
        }

        /// <summary>
        /// Przenosi kontener z jednego statku na inny
        /// </summary>
        private static void TransferContainerBetweenShips()
        {
            if (shipCollection.Count < 2)
            {
                Console.WriteLine("Do przeniesienia kontenera potrzebne sa dwa statki");
                return;
            }

            Console.WriteLine("Wybierz statek zrodlowy");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int sourceShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (sourceShipIndex < 0 || sourceShipIndex >= shipCollection.Count ||
                shipCollection[sourceShipIndex].ListOfContainersOnShip.Count == 0)
            {
                Console.WriteLine("Niepoprawny wybor lub brak kontenerow na statku zrodlowym");
                return;
            }

            Console.WriteLine("Wybierz kontener do przeniesienia");
            for (int i = 0; i < shipCollection[sourceShipIndex].ListOfContainersOnShip.Count; i++)
            {
                Console.WriteLine($"{i + 1} {shipCollection[sourceShipIndex].ListOfContainersOnShip[i].SerialNumber}");
            }

            int selectedContainerIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedContainerIndex < 0 ||
                selectedContainerIndex >= shipCollection[sourceShipIndex].ListOfContainersOnShip.Count)
            {
                Console.WriteLine("Niepoprawny wybor kontenera");
                return;
            }

            Console.WriteLine("Wybierz statek docelowy");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                if (i != sourceShipIndex)
                    Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int destinationShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (destinationShipIndex < 0 || destinationShipIndex >= shipCollection.Count ||
                destinationShipIndex == sourceShipIndex)
            {
                Console.WriteLine("Niepoprawny wybor statku docelowego");
                return;
            }

            Container containerToTransfer =
                shipCollection[sourceShipIndex].ListOfContainersOnShip[selectedContainerIndex];
            try
            {
                shipCollection[destinationShipIndex].AddContainer(containerToTransfer);
                shipCollection[sourceShipIndex].RemoveContainer(containerToTransfer.SerialNumber);
                Console.WriteLine(
                    $"Kontener {containerToTransfer.SerialNumber} zostal przeniesiony ze statku {sourceShipIndex + 1} na statek {destinationShipIndex + 1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blad przy przenoszeniu kontenera {ex.Message}");
            }
        }

        /// <summary>
        /// Wyswietla informacje o kontenerze na podstawie numeru seryjnego
        /// </summary>
        private static void PrintContainerInfo()
        {
            Console.WriteLine("Wprowadz numer seryjny kontenera");
            string containerSerial = Console.ReadLine();
            Container foundContainer = freeContainerCollection.Find(c => c.SerialNumber == containerSerial);
            if (foundContainer == null)
            {
                foreach (var ship in shipCollection)
                {
                    foundContainer = ship.ListOfContainersOnShip.Find(c => c.SerialNumber == containerSerial);
                    if (foundContainer != null)
                        break;
                }
            }

            if (foundContainer != null)
                Console.WriteLine(foundContainer.ToString());
            else
                Console.WriteLine("Nie znaleziono kontenera");
        }

        /// <summary>
        /// Wyswietla informacje o statku i ladunku
        /// </summary>
        private static void PrintShipInfo()
        {
            if (shipCollection.Count == 0)
            {
                Console.WriteLine("Brak statkow");
                return;
            }

            Console.WriteLine("Wybierz statek ktorego informacje chcesz wyswietlic");
            for (int i = 0; i < shipCollection.Count; i++)
            {
                Console.WriteLine($"{i + 1} Statek {i + 1}");
            }

            int selectedShipIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedShipIndex < 0 || selectedShipIndex >= shipCollection.Count)
            {
                Console.WriteLine("Niepoprawny wybor");
                return;
            }

            shipCollection[selectedShipIndex].PrintShipInfo(selectedShipIndex + 1);
        }
    }
}

#endregion