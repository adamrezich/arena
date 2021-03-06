using System;
using System.Collections;
using System.Collections.Generic;
using Cairo;
using VGame;
using Arena;

namespace ArenaClient {
	public class MatchLoadingScreen : State {
		TimeSpan FakeLoadingTime;
		TimeSpan? FakeLoadingDone = null;
		TimeSpan UpdateLoadingTime = TimeSpan.FromSeconds(0.5);
		TimeSpan NextUpdate = TimeSpan.Zero;
		public MatchLoadingScreen() {
			if (Client.Local.IsLocalServer)
				FakeLoadingTime = TimeSpan.FromSeconds(0.1);
			else
				FakeLoadingTime = TimeSpan.FromSeconds(4 + Config.Random.Next(1));
		}
		public override void Update(GameTime gameTime) {
			if (!FakeLoadingDone.HasValue)
				FakeLoadingDone = gameTime.TotalGameTime + FakeLoadingTime;
			if (gameTime.TotalGameTime > NextUpdate) {
				Client.Local.SendLoadingPercent();
				NextUpdate = gameTime.TotalGameTime + UpdateLoadingTime;
			}
			Client.Local.LocalPlayer.LoadingPercent = 1 - (float)Math.Max(Math.Min(((TimeSpan)FakeLoadingDone - gameTime.TotalGameTime).TotalMilliseconds / FakeLoadingTime.TotalMilliseconds, 1), 0);
			if (Client.Local.LocalPlayer.LoadingPercent == 1)
				Client.Local.DoneLoading();
			if (Client.Local.IsLocalServer)
				Server.Local.Update(gameTime);
			Client.Local.Update(gameTime, Vector2.Zero, Vector2.Zero);
			if (Client.Local.Match.Started) {
				Console.WriteLine("[C] Moving to MatchScreen...");
				StateManager.ReplaceState(new MatchScreen());
			}
		}
		public override void Draw(GameTime gameTime) {
			Renderer.Clear(new Color(0.83, 0.83, 0.83));
			Cairo.Context g = Renderer.Context;
			Vector2 origin = new Vector2(Renderer.Width / 2, 8);
			int offset = 0;
			foreach (KeyValuePair<int, Player> kvp in Client.Local.Players) {
				Vector2 pOrigin = origin + new Vector2(0, 20 * (offset + 1));
				g.MoveTo((pOrigin + new Vector2(-100, 0)).ToPointD());
				g.LineTo((pOrigin + new Vector2(-100 + (200 * kvp.Value.LoadingPercent), 0)).ToPointD());
				g.LineTo((pOrigin + new Vector2(-100 + (200 * kvp.Value.LoadingPercent), 20)).ToPointD());
				g.LineTo((pOrigin + new Vector2(-100, 20)).ToPointD());
				g.ClosePath();
				Cairo.Color col = Config.NeutralColor1;
				if (kvp.Value.Team == Teams.Home)
					col = Config.HomeColor1;
				if (kvp.Value.Team == Teams.Away)
					col = Config.AwayColor1;
				Renderer.StrokeAndFill(col, null);
				g.MoveTo((pOrigin + new Vector2(-100, 0)).ToPointD());
				g.LineTo((pOrigin + new Vector2(100, 0)).ToPointD());
				g.LineTo((pOrigin + new Vector2(100, 20)).ToPointD());
				g.LineTo((pOrigin + new Vector2(-100, 20)).ToPointD());
				g.ClosePath();
				Renderer.StrokeAndFill(null, HUD.MainTextStroke);
				Renderer.DrawText(pOrigin + new Vector2(0, 2), kvp.Value.Name, 20, TextAlign.Center, TextAlign.Top, HUD.MainTextFill, HUD.MainTextStroke, null, 0, "chunky");
				offset++;
			}
			Client.Local.DrawChat(Renderer, new Vector2(8, Renderer.Height - 8), 10);
		}
	}
}