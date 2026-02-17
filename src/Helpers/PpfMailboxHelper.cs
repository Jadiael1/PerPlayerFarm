using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace PerPlayerFarm.Helpers
{
    public static class PpfMailboxHelper
    {
        private static bool TryGetPpfOwnerId(GameLocation? location, out long ownerId)
        {
            ownerId = 0;

            string? name = location?.NameOrUniqueName;
            if (string.IsNullOrEmpty(name) || !name.StartsWith("PPF_", StringComparison.Ordinal))
                return false;

            return long.TryParse(name.AsSpan("PPF_".Length), out ownerId);
        }

        private static bool TryGetFarmhouseMailboxTile(Farm farm, out Point mailboxTile)
        {
            mailboxTile = Point.Zero;

            Building? house = farm.buildings
                .FirstOrDefault(b => string.Equals(b.buildingType?.Value, "Farmhouse", StringComparison.OrdinalIgnoreCase));

            if (house is null)
                return false;

            Point doorTile = house.getPointForHumanDoor();
            mailboxTile = new Point(doorTile.X + 4, doorTile.Y + 2);
            return true;
        }

        public static void PrepareMailboxForPpfRender(MailboxState state)
        {
            if (!Context.IsWorldReady)
                return;

            if (!TryGetPpfOwnerId(Game1.currentLocation, out _))
                return;

            if (state.Suppressing)
                return;

            try
            {
                state.MailBackup = Game1.mailbox.ToList();
                state.HadMail = state.MailBackup.Count > 0;

                // Hide vanilla icon in this frame render.
                Game1.mailbox.Clear();
                state.Suppressing = true;
            }
            catch
            {
                Mailbox.ResetMailboxState(state);
            }
        }

        public static void RestoreMailboxAfterPpfRender(MailboxState state)
        {
            if (!state.Suppressing)
                return;

            if (state.MailBackup is not null)
            {
                Game1.mailbox.Clear();
                foreach (string mail in state.MailBackup)
                    Game1.mailbox.Add(mail);
            }

            Mailbox.ResetMailboxState(state);
        }

        public static void DrawOwnerMailboxIconIfNeeded(MailboxState state, SpriteBatch spriteBatch)
        {
            if (!state.Suppressing || !Context.IsWorldReady)
                return;

            if (!state.HadMail)
                return;

            if (!TryGetPpfOwnerId(Game1.currentLocation, out long ownerId))
                return;

            // Só o dono da PPF vê o ícone de mail novo ali
            if (Game1.player.UniqueMultiplayerID != ownerId)
                return;

            if (Game1.currentLocation is not Farm farm)
                return;

            if (!TryGetFarmhouseMailboxTile(farm, out Point tile))
                return;

            DrawMailboxIcon(spriteBatch, tile);
        }

        private static void DrawMailboxIcon(SpriteBatch spriteBatch, Point tile)
        {
            Vector2 basePosition = new(tile.X * Game1.tileSize, tile.Y * Game1.tileSize);
            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            float drawLayer = ((tile.X + 1) * 64f) / 10000f + (tile.Y * 64f) / 10000f;

            Vector2 topOffset = new(0f, -96f - 48f);
            spriteBatch.Draw(
                Game1.mouseCursors,
                Game1.GlobalToLocal(Game1.viewport, basePosition + topOffset + new Vector2(0f, yOffset)),
                new Rectangle(141, 465, 20, 24),
                Color.White * 0.75f,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                drawLayer + 1E-06f
            );

            Vector2 letterOffset = new(32f + 4f, -64f - 24f - 8f);
            spriteBatch.Draw(
                Game1.mouseCursors,
                Game1.GlobalToLocal(Game1.viewport, basePosition + letterOffset + new Vector2(0f, yOffset)),
                new Rectangle(189, 423, 15, 13),
                Color.White,
                0f,
                new Vector2(7f, 6f),
                4f,
                SpriteEffects.None,
                drawLayer + 1E-05f
            );
        }

        public static bool TryHandlePpfMailboxAction(IModHelper helper, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return false;

            if (!e.Button.IsActionButton())
                return false;

            if (!TryGetPpfOwnerId(Game1.currentLocation, out long ownerId))
                return false;

            if (Game1.currentLocation is not Farm farm)
                return false;

            if (!TryGetFarmhouseMailboxTile(farm, out Point mailboxTile))
                return false;

            Point grabTile = Game1.player.GetGrabTile().ToPoint();
            Point playerTile = Game1.player.TilePoint;

            int dxGrab = Math.Abs(grabTile.X - mailboxTile.X);
            int dyGrab = Math.Abs(grabTile.Y - mailboxTile.Y);

            int dxPlayer = Math.Abs(playerTile.X - mailboxTile.X);
            int dyPlayer = Math.Abs(playerTile.Y - mailboxTile.Y);

            // Captures "stuck" and "almost stuck" interaction (alternative vanilla case after trip/facing)
            bool inMailboxInteractionZone =
                (dxGrab <= 1 && dyGrab <= 1) ||
                (dxPlayer <= 1 && dyPlayer <= 1);

            if (!inMailboxInteractionZone)
                return false;

            // Visitor: blocks the use of another player's mailbox.
            if (Game1.player.UniqueMultiplayerID != ownerId)
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
                helper.Input.Suppress(e.Button);
                return true;
            }

            // Owner: uses vanilla mailbox (opens menu/letters normally)
            var loc = new xTile.Dimensions.Location(mailboxTile.X, mailboxTile.Y);
            Game1.currentLocation.performAction("Mailbox", Game1.player, loc);
            helper.Input.Suppress(e.Button);
            return true;
        }
    }
}
