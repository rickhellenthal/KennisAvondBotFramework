# Lab 02 - Koppeling Cognitive Services
In dit lab leer je hoe je een cognitieve service kan koppelen aan je bot om deze intelligenter te maken.
Dit doen we door de QnA Maker service van Microsoft te gebruiken. Dit stelt de bot in staat om bepaalde vragen van een gebruiker te kunnen beantwoorden.

Er is [een uitwerking](./FinishedSolutions) beschikbaar voor het geval je ergens vast komt te zitten.

## **Prerequisites**
- Visual Studio 2017 of nieuwer.
- Een uitwerking van Lab 1, clone de [gegeven uitwerking](./FinishedSolutions/Lab01) indien je hier niet uit bent gekomen.


---


## **Opdracht 1 - QnA Maker**

**1.1 - QnA Maker portaal)**

De eerste stap is het opzetten en inrichten van de **knowledge base** in het [QnA Maker Portaal](https://www.qnamaker.ai/).
- Log in bij het portaal en druk op _Create a knowledge base_.
- Volg de stappen de in het portaal worden uitgelegd, een paar toevoegingen hierbij:
    - Bij stap 1:
        - Selecteer _F0_ bij _pricing tier_
        - Selecteer (indien mogelijk) _F_ bij _search pricing tier_, kies anders voor _B_
        - Zet _App insights_ uit
    - Bij stap 4:
        - Het inladen van een document kun je overslaan, we gaan handmatig vragen toevoegen. Indien gewenst kan je _chit-chat_ toevoegen aan je knowledge base. Dit is echter alleen beschikbaar in het Engels.
- Voeg een aantal vragen en antwoorden naar keuze toe, merk op dat het mogelijk is om alternatieve verwoordingen te definiëren bij vragen.
- Druk op _Save and train_.
- Publiceer je service.


**1.2 - Voorbereidend werk koppeling met bot)**

- Installeer de Microsoft.Bot.Builder.AI.QnA NuGet package.
- Voeg aan `appsettings.json` de volgende regels toe:
    ```C#
    "QnAKnowledgebaseId": "<your-knowledge-base-id>",
    "QnAAuthKey": "<your-knowledge-base-endpoint-key>",
    "QnAEndpointHostName": "<your-qna-service-hostname>"
    ```
    Deze waardes die je hier dient in te vullen zijn terug te vinden in het [QnA Maker Portaal](https://www.qnamaker.ai/) onder _Deployment details_ onder _SETTINGS_.

**1.3 - Koppeling QnA Service en Bot)**

Nu de knowledge base van de QnA Maker is aangevuld en het voorbereidend werk gedaan is, kan de bot in staat worden gesteld om gebruik te maken van de service.

- Registreer een nieuw QnAMakerEndpoint in de `ConfigureServices()` functie in `Startup.cs`, gebruik hiervoor de onderstaande code.

    ```C#
    // Create QnAMaker endpoint as a singleton
    services.AddSingleton(new QnAMakerEndpoint
    {
        KnowledgeBaseId = Configuration.GetValue<string>($"QnAKnowledgebaseId"),
        EndpointKey = Configuration.GetValue<string>($"QnAAuthKey"),
        Host = Configuration.GetValue<string>($"QnAEndpointHostName")
    });
    ```

Doordat je zojuist de QnAMakerEndpoint hebt geregistreerd als singleton in `Startup.cs` wordt deze door middel van dependency injection meegegeven aan de contstructor van `KennisAvondBot`. 
- Voeg `QnAMakerEndpoint endpoint` toe als parameter in zowel de constructor van **KennisAvondBot** als die van **QuestionDialog**.
- Geef deze **endpoint** door aan **QuestionDialog** bij de initialisatie hiervan in de constructor van **KennisAvondBot**.
- Voeg de property `QnAMaker _kennisAvondBotQnA` toe aan **QuestionDialog**.
- In de constructor van **QuestionDialog**, voeg `_kennisAvondBotQnA = new QnAMaker(endpoint);` toe.


**1.4 - Aanroepen QnA Maker Service)**

Alles staat nu klaar om de QnA Maker service aan te kunnen roepen. In lab 1 heb je de stap `AnswerQuestionStepAsync()` aangemaakt in `QuestionDialog`, hier werken we nu op door.

- Vervang de implementatie van de functie door het volgende:
    - Benader de QnA service om te kijken of deze de vraag van de gebruiker herkent en een antwoord erop heeft, gebruik hiervoor de `GetAnswersAsync()` functie.
    - Indien er een antwoord is gevonden, stuur dit antwoord als een bericht naar de gebruiker.
    - Indien er geen antwoord is gevonden, stuur iets als _'Sorry, ik heb geen antwoord op de vraag "{vraag}" kunnen vinden.'_.
    - Beëindig het dialoog.


---


## **Opdracht 2 - En we zijn live!**

Nu de bot functionaliteit heeft, is het tijd om hem te publiceren!

- Maak in het [Azure portaal](https://portal.azure.com) een nieuwe _Web App Bot_ resource aan.
    - Zet de _location_ op _West Europe_.
    - Bij _pricing tier_, druk op _View full pricing details_ en selecteer (indien mogelijk) _F0 Free_.
    - Kies bij _Bot Template_ voor _Echo Bot_. Deze code wordt dadelijk overschreven door de code die we gemaakt hebben tijdens de labs. De reden dat we hier voor _Echo Bot_ kiezen als template is omdat er anders een extra resource (LUIS App) wordt aangemaakt.
    - Kies dezelfde _App sevice plan_ als die je gebruikt voor de QnA Maker Service.


- In Visual Studio, rechtermuisklik op het project (niet de solution) en druk op _publish_.
- Maak een nieuw _Publish profile_ aan door op _Start_ te drukken.
- Kies de **bestaande** Azure App Service die je zojuist hebt aangemaakt.
- Publiceer je bot.


In het Azure portaal, bij de Web App Bot resource kan je onder _Bot management_ de functionaliteit _Test in Web Chat_ terugvinden.
Zowel hier, als door middel van de emulator, kun je de gepubliceerde bot testen.

- Test je bot in de Web Chat op Azure of via de bot Emulator.


---


## **Klaar**

Gefelicteerd! Je hebt nu kennis van het Microsoft Bot Framework en bent in staat hiermee te ontwikkelen, lekker bezig!
Wil je toch nog even door? Dan kan je je bot nog uitbreiden met LUIS [door middel van deze uitleg](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-v4-luis?view=azure-bot-service-4.0&tabs=csharp). LUIS is een op machine learning gebaseerde service om het begrip van natuurlijke taal te integeren.

