﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace TerraVoice.UI.ControlPanel;

internal sealed class TextBanner
{
    private const int TimerCooldown = 180;

    private readonly string text;

    private readonly DynamicSpriteFont font;

    private bool scrollingRequired;

    private float maximumOffset;

    private float offset;

    private float scrollTimer;

    private bool reversing;

    public TextBanner(string text, DynamicSpriteFont font)
    {
        this.text = text;
        this.font = font;
    }

    public void UpdateScrolling(Rectangle scissorRectangle)
    {
        float textWidth = font.MeasureString(text).X;

        scrollingRequired = textWidth > scissorRectangle.Width - 8;
        maximumOffset = scissorRectangle.Width - 8 - textWidth;

        if (!reversing)
        {
            if (scrollTimer < TimerCooldown)
            {
                scrollTimer++;
                return;
            }

            if (offset > maximumOffset)
            {
                offset -= 1;
            }

            if (offset <= maximumOffset)
            {
                offset = maximumOffset;

                reversing = true;
            }
        }
        else
        {
            if (scrollTimer > 0)
            {
                scrollTimer--;
                return;
            }

            offset = 0;
            reversing = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Rectangle scissorRectangle, Color color)
    {
        if (!scrollingRequired)
        {
            spriteBatch.DrawString(font, text, position, color);
        }
        else
        {
            RasterizerState state = new()
            {
                ScissorTestEnable = true,
                CullMode = CullMode.None
            };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, state,
                null, Main.UIScaleMatrix);

            float xScale = Main.UIScaleMatrix.M11;
            float yScale = Main.UIScaleMatrix.M22;

            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(
                (int)(scissorRectangle.X * xScale),
                (int)(scissorRectangle.Y * yScale),
                (int)(scissorRectangle.Width * xScale),
                (int)(scissorRectangle.Height * yScale)
            );

            spriteBatch.DrawString(font, text, position + new Vector2(offset, 0), color);

            Main.instance.GraphicsDevice.ScissorRectangle = Main.instance.GraphicsDevice.Viewport.Bounds;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.UIScaleMatrix);
        }
    }
}
