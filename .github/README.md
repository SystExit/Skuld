# Skuld
[![Build Status](https://dev.azure.com/exsersewo/Skuldbot/_apis/build/status/skuldbot.Skuld?branchName=master)](https://dev.azure.com/exsersewo/Skuldbot/_build/latest?definitionId=1&branchName=master)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/cc4d741e85194c3291da05dd1f36ff3e)](https://www.codacy.com/gh/skuldbot/Skuld?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=skuldbot/Skuld&amp;utm_campaign=Badge_Grade)
[![Discord.Net](https://img.shields.io/badge/Library-Discord.Net-blue)](https://github.com/discord-net/Discord.Net)

A Discord bot that helps being fun and interaction to your server through means of fun commands, gamification & moderation.

## Self Hosting
###### Prerequisites
* Visual Studio >= 2019
* MariaDB >= 10.4.12
* Net Core Version >= 3.1
#### Development Builds
1. Clone the [dev](https://github.com/skuldbot/skuld/tree/dev/) branch
2. Clone all submodules
3. Build the project
4. Follow the steps for Consumer Releases from Step 2. onwards
#### Consumer Releases
1. Download the [latest release](https://github.com/skuldbot/skuld/releases/latest)
2. Set the Environment Variable `SKULD_CONNSTR` with your connection string
3. Start the bot and let it generate a configuration entry
4. In your database management of choice (like [HeidiSQL](https://www.heidisql.com/) or [TablesPlus](https://www.tableplus.io/)) enter your tokens into the only row in `Configuration`
5. Restart the bot
6. 🎊 You now have a working instance of Skuld 🎊