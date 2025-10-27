import discord
from discord.ext import commands
from datetime import timedelta
from dotenv import load_dotenv
import asyncio
import random
import os

load_dotenv()

# Landmine options
EXPLOSION_CHANCE = .01
TIMEOUT_DURATION = timedelta(minutes = 1)

# Landmine storage
land_mines = []
land_mine_lock = asyncio.Lock()

# Discord intent stuff
intents = discord.Intents.default()
intents.message_content = True
intents.members = True

token = os.getenv("discordToken")

bot = commands.Bot(command_prefix='$', intents=intents)
@bot.event
async def on_ready():
    print(f'Logged on as {bot.user}!')
    await bot.tree.sync()

@bot.event
async def on_message(message):
    if message.author.bot:
        return

    async with land_mine_lock:
        member = message.guild.get_member(message.author.id)
        if len(land_mines) > 0:
            mines = [mine for mine in land_mines if mine["channel"] == message.channel]
            mine = random.choice(mines)
            
            if random.random() < EXPLOSION_CHANCE:
                try:
                    await member.edit(
                        timed_out_until = discord.utils.utcnow() + TIMEOUT_DURATION,
                        reason="💣 KABOOOOOOOOOOM"
                    )
                    await message.channel.send(
                        f"💣💣💣💣 {member.mention} stepped on a landmine placed by {mine['owner'].mention}! 💣💣💣💣"
                    )
                except discord.Forbidden:
                    await message.channel.send(
                        f"{member.mention} stepped on a landmine placed by {mine['owner'].mention} but could not be timed out!"
                    )
                except discord.HTTPException as e:
                    print(f"Failed to timeout user: {e}")
            await bot.process_commands(message)

            land_mines.remove(mine)
        else:
            return

# Places a landmine in the current channel this message was sent in
@bot.tree.command(name="land_mine", description="Place a landmine in this channel.")
async def land_mine(interaction: discord.Interaction):
   payload = {
       "channel": interaction.channel,
       "owner": interaction.user
              }
   land_mines.append(payload)
   await interaction.response.send_message(f"Landmine has been placed in {interaction.channel.name} by {interaction.user}")

bot.run(token)