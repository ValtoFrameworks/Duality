﻿using System;
using System.Collections.Generic;
using System.Linq;

using Duality;
using Duality.Resources;
using Duality.Drawing;

namespace DualStickSpaceShooter
{
	public class HeadUpDisplay : Component, ICmpRenderer
	{
		private ContentRef<Font>       font      = null;
		[DontSerialize] private Player playerOne = null;
		[DontSerialize] private Player playerTwo = null;
		[DontSerialize] private Canvas canvas    = null;

		public ContentRef<Font> Font
		{
			get { return this.font; }
			set { this.font = value; }
		}

		void ICmpRenderer.GetCullingInfo(out CullingInfo info)
		{
			info.Position = Vector3.Zero;
			info.Radius = float.MaxValue;
			info.Visibility = VisibilityFlag.AllGroups | VisibilityFlag.ScreenOverlay;
		}
		void ICmpRenderer.Draw(IDrawDevice device)
		{
			// Create a Canvas for high-level drawing commands.
			// We'll re-use this to keep performance high and allocations low.
			if (this.canvas == null) this.canvas = new Canvas();

			// Prepare the canvas for drawing
			this.canvas.Begin(device);
			this.canvas.State.SetMaterial(DrawTechnique.Alpha);
			this.canvas.State.TextFont = this.font;

			// Retrieve players
			if (this.playerOne == null)
				this.playerOne = this.Scene.FindComponents<Player>().Where(p => p.Id == PlayerId.PlayerOne).FirstOrDefault();
			if (this.playerTwo == null)
				this.playerTwo = this.Scene.FindComponents<Player>().Where(p => p.Id == PlayerId.PlayerTwo).FirstOrDefault();

			// Is someone playing using mouse / keyboard? Display a mouse cursor then
			if (Player.AlivePlayers.Any(p => p.InputMethod == InputMethod.MouseAndKeyboard))
			{
				this.canvas.FillCircle(DualityApp.Mouse.Pos.X, DualityApp.Mouse.Pos.Y, 2.0f);
			}

			// Is any player alive? Keep that value in mind, won't change here anyway.
			bool isAnyPlayerAlive = Player.IsAnyPlayerAlive;

			// Draw health and info of player one
			if (this.IsPlayerActive(this.playerOne))
			{
				Ship playerShip = this.playerOne.ControlObject;

				if (playerShip.Active)
				{
					// Draw a health bar when alive
					float health = playerShip.Hitpoints;

					this.canvas.State.ColorTint = ColorRgba.Black.WithAlpha(0.5f);
					this.canvas.FillRect(12 - 1, device.TargetSize.Y - 10 - 198 - 1, 16 + 2, 196 + 2);

					this.canvas.State.ColorTint = this.playerOne.Color;
					this.canvas.DrawRect(10, device.TargetSize.Y - 10 - 200, 20, 200);
					this.canvas.FillRect(12, device.TargetSize.Y - 10 - health * 198.0f, 16, health * 196.0f);
				}
				else if (isAnyPlayerAlive && !this.playerOne.HasReachedGoal)
				{
					// Draw a respawn timer when dead
					float respawnPercentage = this.playerOne.RespawnTime / Player.RespawnDelay;
					string respawnText = string.Format("Respawn in {0:F1}", (Player.RespawnDelay - this.playerOne.RespawnTime) / 1000.0f);
					Vector2 textSize = this.canvas.MeasureText(string.Format("Respawn in {0:F1}", 0.0f));

					this.canvas.State.ColorTint = ColorRgba.Black.WithAlpha(0.5f);
					this.canvas.FillRect(10 - 1, device.TargetSize.Y - 10 - textSize.Y - 2, textSize.X + 5, textSize.Y + 8);

					this.canvas.State.ColorTint = this.playerOne.Color;
					this.canvas.DrawText(respawnText, 10, device.TargetSize.Y - 10, 0.0f, Alignment.BottomLeft);
					this.canvas.FillRect(10, device.TargetSize.Y - 10 - textSize.Y, textSize.X * respawnPercentage, 3);
					this.canvas.FillRect(10, device.TargetSize.Y - 10, textSize.X * respawnPercentage, 3);
				}
			}

			// Draw health and info of player two
			if (this.IsPlayerActive(this.playerTwo))
			{
				Ship playerShip = this.playerTwo.ControlObject;

				if (playerShip.Active)
				{
					// Draw a health bar when alive
					float health = playerShip.Hitpoints;

					this.canvas.State.ColorTint = ColorRgba.Black.WithAlpha(0.5f);
					this.canvas.FillRect(device.TargetSize.X - 12 - 16 - 1, device.TargetSize.Y - 10 - 198 - 1, 16 + 2, 196 + 2);

					this.canvas.State.ColorTint = this.playerTwo.Color;
					this.canvas.DrawRect(device.TargetSize.X - 10 - 20, device.TargetSize.Y - 10 - 200, 20, 200);
					this.canvas.FillRect(device.TargetSize.X - 12 - 16, device.TargetSize.Y - 10 - health * 198.0f, 16, health * 196.0f);
				}
				else if (isAnyPlayerAlive && !this.playerTwo.HasReachedGoal)
				{
					// Draw a respawn timer when dead
					float respawnPercentage = this.playerTwo.RespawnTime / Player.RespawnDelay;
					string respawnText = string.Format("{0:F1} to Respawn", (Player.RespawnDelay - this.playerTwo.RespawnTime) / 1000.0f);
					Vector2 textSize = this.canvas.MeasureText(string.Format("{0:F1} to Respawn", 0.0f));

					this.canvas.State.ColorTint = ColorRgba.Black.WithAlpha(0.5f);
					this.canvas.FillRect(device.TargetSize.X - 10 - textSize.X - 3, device.TargetSize.Y - 10 - textSize.Y - 2, textSize.X + 2, textSize.Y + 10);

					this.canvas.State.ColorTint = this.playerTwo.Color;
					this.canvas.DrawText(respawnText, device.TargetSize.X - 10, device.TargetSize.Y - 10, 0.0f, Alignment.BottomRight);
					this.canvas.FillRect(device.TargetSize.X - 10 - textSize.X * respawnPercentage, device.TargetSize.Y - 10 - textSize.Y, textSize.X * respawnPercentage, 3);
					this.canvas.FillRect(device.TargetSize.X - 10 - textSize.X * respawnPercentage, device.TargetSize.Y - 10, textSize.X * respawnPercentage, 3);
				}
			}

			this.canvas.End();
		}

		private bool IsPlayerActive(Player player)
		{
			return
				player != null &&
				player.Active &&
				player.IsPlaying &&
				player.ControlObject != null;
		}
	}
}
