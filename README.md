# Hospital WebApi - PJATK APBD 2026, Tutorial 8

Małe API szpitala napisane w .NET 9 z EF Core. Baza jest pierwsza - model wyciągnąłem
z gotowej bazy podejściem DatabaseFirst, więc kod nie tworzy schematu, tylko go używa.

## Jak odpalić

Najpierw trzeba mieć bazę. Odpalasz `create.sql` na swoim SQL Serverze - skrypt zakłada
tabele i wrzuca dane przykładowe. Potem sprawdź, czy connection string `Default`
w `appsettings.json` pasuje do twojego serwera (domyślnie celuje w `localhost:1433`,
baza `HospitalDb`).

Reszta to już zwykłe:

```bash
dotnet run
```

API słucha na `http://localhost:5203`.

## Skąd wzięły się modele

Kontekst i klasy w `Models/` nie są pisane ręcznie - wygenerował je scaffold z istniejącej bazy:

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Default" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --context HospitalDbContext --output-dir Models
```

## Co API potrafi

**`GET /api/patients`** - oddaje wszystkich pacjentów razem z ich przyjęciami i przypisaniami
łóżek. Można dorzucić `?search=`, wtedy lecimy `LIKE %...%` po imieniu i nazwisku:

```
GET /api/patients
GET /api/patients?search=an
```

**`POST /api/patients/{pesel}/bedassignments`** - przydziela pacjentowi wolne łóżko danego typu
w danym oddziale na wybrany okres. Jeśli czegoś brakuje (pacjenta, oddziału, typu łóżka albo
po prostu nie ma wolnego łóżka), dostajesz `404` z konkretnym komunikatem, a nie jednym
ogólnikiem na wszystko.

```json
{
  "from": "2026-05-20T14:00:00",
  "to": "2026-05-30T10:00:00",
  "bedType": "Standard",
  "ward": "Kardiologia"
}
```

`to` możesz pominąć - wtedy przypisanie jest otwarte (bez daty końca). Gotowe przykłady
zapytań leżą w `PJATK-APBD-Cw5-s32103.http`.
