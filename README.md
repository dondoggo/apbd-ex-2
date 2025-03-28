# apbd-ex-2

**System Zarządzania Ładunkiem Kontenerów

Aplikacja w języku C#, która symuluje zarządzanie ładunkiem kontenerów.

Struktura Projektu
	•	Container – klasa bazowa dla kontenerów, zawiera wspólne właściwości i metody.
	•	LiquidContainer, GasContainer, RefrigeratedContainer – klasy dziedziczące po kontenerze, reprezentujące różne typy kontenerów z dodatkowymi ograniczeniami.
	•	Ship – klasa reprezentująca statek, który może przewozić kontenery.
	•	Program – główna klasa z interfejsem konsolowym, umożliwiająca wybór opcji i wykonywanie operacji.

Przykładowe Funkcje

Podczas uruchomienia aplikacji użytkownik widzi menu z opcjami, takimi jak:
	•	Dodaj statek
	•	Usuń statek
	•	Stwórz kontener
	•	Załaduj ładunek do kontenera
	•	Umieść kontener na statku
	•	Umieść listę kontenerów na statku
	•	Usuń kontener ze statku
	•	Rozładuj kontener
	•	Zastąp kontener na statku
	•	Przenieś kontener między statkami
	•	Wyświetl informacje o kontenerze
	•	Wyświetl informacje o statku i ładunku
