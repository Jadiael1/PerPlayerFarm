using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerPlayerFarm.Events.ButtonPressed;
using PerPlayerFarm.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.RenderedWorld
{
    public class RenderedWorld
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly string _facadeBuildingId;
        private readonly ITranslationHelper _translate;
        private MailboxState _mailboxState;

        public RenderedWorld(IModHelper helper, IMonitor monitor, MailboxState mailboxState)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _facadeBuildingId = Utils.Constants.FacadeBuildingId;
            _mailboxState = mailboxState;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
        }

        private void ResetState()
        {
            _mailboxState.MailBackup = null;
            _mailboxState.Suppressing = false;
            _mailboxState.HadMail = false;
        }

        private void RestoreMailbox()
        {
            if (_mailboxState.MailBackup is not null)
            {
                Game1.mailbox.Clear();
                foreach (string mail in _mailboxState.MailBackup)
                    Game1.mailbox.Add(mail);
            }

            ResetState();
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
                drawLayer + 1E-06f);

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
                drawLayer + 1E-05f);
        }

        private bool TryGetMailboxTileForOwner(GameLocation location, out Point mailboxTile)
        {
            mailboxTile = Point.Zero;
            foreach (var building in location.buildings)
            {
                if (building is null || building.buildingType?.Value != _facadeBuildingId)
                    continue;

                if (!PpfBuildingHelper.TryGetOwnerUid(building, location, out long ownerUid))
                    continue;

                if (ownerUid != Game1.player.UniqueMultiplayerID)
                    continue;

                mailboxTile = PpfBuildingHelper.GetMailboxTile(building);
                return true;
            }
            return false;
        }

        private static bool IsPerPlayerFarm(string? name)
            => name is not null && name.StartsWith("PPF_", StringComparison.Ordinal);

        public void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            try
            {
                if (!_mailboxState.Suppressing || !Context.IsWorldReady)
                    return;

                var location = Game1.currentLocation;
                if (!IsPerPlayerFarm(location?.NameOrUniqueName) || !_mailboxState.HadMail)
                    return;

                if (!TryGetMailboxTileForOwner(location!, out Point mailboxTile))
                    return;

                DrawMailboxIcon(e.SpriteBatch, mailboxTile);
            }
            finally
            {
                RestoreMailbox();
            }
        }



    }
}