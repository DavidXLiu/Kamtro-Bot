# Kamtro Bot
## What is Kamtro?
Kamtro Bot is a bot created for the musicians Kamex and RetroSpecter's Joint Discord server

## Getting Started
A full list of commands, with examples and descriptions is available in the interactive help menu, just use `!help` to get started!

## Features:
Kamtro Bot comes equipped with several commands, notable features include:
- Customizable reminders, adjusted to your specified time zone
- Items
- Shop
- Crafting
- Achievements (titles)
- Spam filter
- User Profiles
- Activity tracking
- Reputation score

Almost all features listed above are used via an interactive menu, using discord’s Reactions feature as buttons.

## Inner workings:
Features of kamtro bot are designed to be modular and easier to implement, while still remaining efficient. 

The interactive menus are all subclasses of one of the main types of menus, these are all called embeds.
- Non-abstract classes that extend `KamtroEmbedBase` are embeds that have no interaction, and only display text.
- Non-abstract Classes which extend `ActionEmbed` are ones whose interaction is limited to the buttons at the bottom (the reactions automatically added by the bot)
- Classes which extend `MessageEmbed` are embeds which rely on both button input and text input, sent by the user as a message. There are no embeds which rely only on text input and have no buttons, as there will always be a close embed button, and usually some form of a confirm button.
    - Message embeds use reflection along with a custom attribute (AKA decorator) to ensure that the variable on the proper page and position is set according to user input. 
- Many complicated features are simplified into classes which perform the necessary tasks required by other classes, these are called Managers.
- The most used managers are for user data and user inventories.
- User inventories are stored in the user data file as an array of numerical IDs. These IDs map to specific item objects defined in a JSON file in a folder near the executable (not in this repo)
- Titles also make use of IDs, in much the same way as items
