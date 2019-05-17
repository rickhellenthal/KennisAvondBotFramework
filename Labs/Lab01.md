# Lab 01 - Basic Bot met Dialogs
In dit lab maak je kennis met het Microsoft Bot Framework door een chat bot te ontwikkelen die een gebruiker kan herkennen.
Deze bot zal gebruik maken van dialogs.

Er is [een uitwerking](./FinishedSolutions) beschikbaar voor het geval je ergens vast komt te zitten.

## **Prerequisites**
- Visual Studio 2017 of nieuwer.
- Het Bot Framework v4 SDK [template voor C#](https://marketplace.visualstudio.com/items?itemName=BotBuilder.botbuilderv4) is geïnstalleerd.
- [Microsoft Bot Framework Emulator](https://github.com/microsoft/BotFramework-Emulator/releases/tag/v4.3.3) is geïnstalleerd.


---


## **Opdracht 1 - Opzetten**
**1.1 - Nieuw project)**

Maak een nieuw project aan in Visual Studio en selecteer hierbij het ‘Empty Bot’ template.

<img src="../Resources/Images/lab01_01.png?raw=true" height="500">


**1.2 - Bot configuratie)**

Maak een .bot bestand aan, dit doe je door een nieuwe bot configuratie in de Bot Framework Emulator te definiëren. Vul hier een naam en de http endpoint in waarop de bot gehost wordt, de rest kan leeg gelaten worden.

<img src="../Resources/Images/lab01_02.png?raw=tru" height="350">


**1.3 - Reactie)**

Implementeer de OnMessageActivityAsync functie. Laat de bot door middel hiervan een reactie geven op een inkomend bericht van een gebruiker.

```C#
protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
{
    // Schrijf hier code om een bericht naar de gebruiker te sturen.
}
```
Start de bot via Visual Studio en open in de emulator een conversatie hiermee. Controleer of de bot ‘Hello World!’ stuurt, en of deze reageert op de input van de gebruiker zoals zojuist gedefinieerd.

<img src="../Resources/Images/lab01_03.png?raw=true" height="200">


---


## **Opdracht 2 - State en accessors**
**2.1 - Definieer classes)**

De eerste stap in het opzetten van de state management is om de classes te definiëren die de benodigde informatie op zullen slaan. Voor deze opdracht hebben we twee classes nodig:
- Maak een class aan genaamd UserProfile, geef deze een public string property `Name`, inclusief getter en setter.
- Maak een class aan genaamd ConversationData, geef deze een public boolean property `PromptedForName`, inclusief getter en setter.


**2.2 - ConversationState en UserState)**

Vervolgens registeren we de MemoryStorage, deze zal gebruikt worden om de UserState en ConversationState objecten op te slaan.
[Memory Storage](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-state?view=azure-bot-service-4.0#storage-layer) is gemaakt voor het lokaal testen van bots. De data in-memory wordt bij elke restart van de bot geleegd.
Daarnaast registreren we de UserState en ConversationState, deze worden bij het opstarten aangemaakt en door middel van dependency injection meegegeven aan de bot constructor.
- Voeg de onderstaande code toe aan de ConfigureServices functie binnen `Startup.cs`

    ```C#
    services.AddSingleton<IStorage, MemoryStorage>();

    services.AddSingleton<UserState>();

    services.AddSingleton<ConversationState>();
    ```

- Voeg de onderstaande contructor en private properties toe aan het `kennisAvondBot.cs` bestand, indien je het project een andere naam hebt gegeven wordt die naam aangehouden bij het genereren van dit bestand. De ConversationState en UserState worden door middel van dependency injection meegegeven aan de constructor.

    ```C#
    private BotState _conversationState;
    private BotState _userState;


    /* 
        Bij het genereren wordt deze class EmptyBot genoemd, indien gewenst kun je dit
        hernoemen zodat dit overeenkomt met de naam van het project.
        Zorg er dan voor dat dit op elke locatie hernoemd wordt.
    */ 
    public EmptyBot(ConversationState conversationState, UserState userState)
    {
        _conversationState = conversationState;
        _userState = userState;
    }
    ```

**2.3 - Accessors)**

Vervolgens maken we twee properties aan door middel van de `CreateProperty()` methode, deze properties zorgen ervoor dat we met de BotState kunnen interacteren. Elke state property accessor zorgt ervoor dat we de key-value pairs kunnen getten en setten van de bijbehorende property.

Voordat we gebruik kunnen maken van onze de state properties, dienen we eerst de property van de opslagmethode in de state cache te laden. Hiervoor roepen we de `GetAsync()` methode aan.

- Voeg de onderstaande code toe aan de `OnMessageActivityAsync()` functie om gebruik te kunnen maken van de conversation state binnen de bot. Doe dit ook voor de user state.

    ```C#
    var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
    var conversationData = await conversationStateAccessors.GetAsync(turnContext,() => new ConversationData());
    ```


**2.4 - Herkennen gebruiker)**

Nu alles klaar voor gebruik is kan er functionaliteit worden toegevoegd aan de bot. Om kennis te maken met hoe dit te werk gaat, zullen we een bot maken die een gebruiker kan herkennen. De gewenste uitvoer is als volgt:

<img src="../Resources/Images/lab01_04.png?raw=true" height="350">

Zorg ervoor dat:
- De bot controleert of de naam van de gebruiker al bekend is of niet.
    - Indien dit niet het geval is de bot checkt of hij de naam van de gebruiker al gevraagd heeft of niet.
        - Indien dit niet het geval is de bot een bericht stuurt met de vraag wat de naam van de gebruiker is.
        - Indien dit wel het geval is de bot de inhoud van het bericht van de gebruiker opslaat als de naam van de gebruiker.
    - Indien dit wel het geval is de bot reageert met iets als ‘Hoi {naam}’.
- De bot na elke turn de eventuele veranderingen opslaat. Gebruik hiervoor de onderstaande code in het `kennisAvondBot.cs` bestand.

    ```C#
    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }
    ```

> Tip: Om een nieuw gesprek te beginnen als dezelfde gebruiker, druk op het uitklapbare pijltje links naast 'Resart conversation' in de Emulator, en selecteer 'Restart with same user ID'.


---


## **Opdracht 3 - Dialogs**


In de vorige opdracht heb je functionaliteit toegevoegd om de naam van de gebruiker te leren kennen. Voor kleine applicaties is het nog rendabel om de functionaliteit in de OnMessageActivityAsync te plaatsen. Echter wordt dit bij bots met meer functionaliteit gauw onoverzichtelijk. Dialogs bieden hier een oplossing voor. In deze opdracht bouwen we de bot om zodat deze gebruik maakt van dialogen.

**3.1 - UserProfileDialog)**

- Installeer de Microsoft.Bot.Builder.Dialogs NuGet package.
- Maak een nieuwe map aan genaamd Dialogs, maak hier een nieuwe UserProfileDialog class aan die ComponentDialog inherit. Vul deze class met de onderstaande contructor en property aan.

    ```C#
    private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

    public UserProfileDialog(UserState userState) : base(nameof(UserProfileDialog))
    {
        _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

        var waterfallSteps = new WaterfallStep[]
        {
            AskForNameStepAsync,
            NameConfirmStepAsync,
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt("NamePrompt"));

        InitialDialogId = nameof(WaterfallDialog);
    }
    ```

In de variabele `waterfallSteps` staan twee functienamen, dit houdt in dat dit dialoog (UserProfileDialog) bestaat uit twee stappen; `AskForNameStepAsync`, en `NameConfirmStepAsync`. Deze zullen in de volgorde uitgevoerd worden waarin ze zijn toegevoegd.

Merk op dat we een nieuw `TextPrompt` aanmaken met de naam `NamePrompt`, door middel van prompts kan informatie aan de gebruiker gevraagd worden. Deze prompts kunnen voorzien worden van verschillende opties en validators. In deze opdracht maken we hier echter geen gebruik van.

- Voeg de onderstaande functie toe aan `UserProfileDialog.cs`.

    ```C#
    private async Task<DialogTurnResult> AskForNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Hallo, volgens mij hebben wij elkaar nog niet ontmoet."));
        return await stepContext.PromptAsync("NamePrompt",
            new PromptOptions
            {
                Prompt = MessageFactory.Text("Wat is je naam?"),
            }, cancellationToken);
    }
    ```

De volgende stap van het dialoog is het opslaan van de naam van de gebruiker. Het resultaat van de vorige stap is door middel van de `stepContext` beschikbaar. Het resultaat wordt in de UserState geplaatst, dit wordt aan het einde van de turn opgeslagen door de code in de `OnTurnAsync()` functie in `kennisAvondBot.cs`.
Omdat dit de laatste stap in het dialoog is, wordt hier als laatste `EndDialogAsync()` aangeroepen om het dialoog te beëindigen.

- Voeg de onderstaande functie toe aan `UserProfileDialog.cs`.

    ```C#
    private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Name = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Super, leuk je te ontmoeten {userProfile.Name}."));
            return await stepContext.EndDialogAsync();
        }
    ```


**3.2 - Dialog aanroepen)**

Nu het dialoog klaar staat, kan deze aangeroepen worden. Om het dialoog aan te roepen hebben we een aantal dingen nodig, deze zullen we nu aanmaken. Als eerste maken we de DialogSet aan, hier voegen we het dialoog aan toe die we zojuist hebben aangemaakt.

- Voeg de onderstaande code toe aan de constructor in het `kennisAvondBot.cs` bestand, definiëer daarnaast ook de `_dialogSet` property in de class.

    ```C#
    var dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(DialogState)));
    dialogSet.Add(new UserProfileDialog(userState));
    _dialogSet = dialogSet;
    ```

Daarnaast maken we de dialogContext aan, hiermee kunnen we interacteren met onze dialogen.
- Voeg de onderstaande code toe aan de `OnMessageActivityAsync()` functie.

    ```C#
    var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
    var results = await dialogContext.ContinueDialogAsync(cancellationToken);
    bool activeDialog = results.Status != DialogTurnStatus.Empty;
    ```


- Vervang de functionaliteit in `OnMessageActivityAsync()` zoals gemaakt in opdracht 2.4 met het volgende:
    - Doe indien er al een dialoog actief is niets. Door de functie `ContinueDialogAsync()` wordt de volgende stap van een actief dialoog in gang gezet, daarom hoeven we verder niets te doen.
    - Start, indien de naam van de gebruiker nog niet bekend is, het zojuist aangemaakte dialoog. Gebruik als dialogId `nameof(UserProfileDialog)`.
    - Indien de naam van de gebruiker wel bekend is, de bot reageert met iets als _'Hoi {naam}'_.



> Tip: Het gebruik van `PromptedForName` uit de ConversationState is nu niet meer nodig.


**3.3 - Nieuw dialoog)**

In lab 2 gaan we een cognitieve service koppelen aan de bot. Als opzet hiervoor hebben we nog een dialoog nodig, in deze opdracht gaan we dit dialoog maken.

Maak een nieuw dialoog aan genaamd `QuestionDialog` en zorg ervoor dat:
- Wanneer de gebruiker _'vraag'_ intypt, het dialoog wordt aangeroepen.
- Dit dialoog als eerste stap aan de gebruiker de vraag stelt _'Wat is je vraag {gebruikersnaam}?'_.
- Dit dialoog als tweede stap reageert met _'Sorry, ik heb geen antwoord op de vraag {vraag}.'_.

> Tip: Gebruik UserProfileDialog als voorbeeld.

Een voorbeeld van de gewenste uitvoer is als volgt:

<img src="../Resources/Images/lab01_05.png?raw=true" height="250">

