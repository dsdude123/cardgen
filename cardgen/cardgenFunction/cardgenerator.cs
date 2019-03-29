using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using cardgenFunction.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace cardgenFunction
{
    public static class cardgenerator
    {
        [FunctionName("GenerateCard")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Incoming card generation request.");

            try
            {
                CardGenerationRequest incoming =
                    JsonConvert.DeserializeObject<CardGenerationRequest>(req.Content.ReadAsStringAsync().Result);
                Image card = null;
                switch (incoming.type)
                {
                    case CardGenerationRequest.CardType.Hearthstone:
                    {
                        //try
                        //{
                            card = GenerateHearthstone(incoming.hearthstoneCard);
                        //}
                        //catch (CardGeneratorException ex)
                        //{
                        //    log.Error("Card generation failed: " + ex.Message);
                        //    CardGenerationResponse r = new CardGenerationResponse();
                        //    r.message = ex.Message;
                        //    HttpResponseMessage rep =
                        //        req.CreateResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(r));
                        //    return rep;
                        //}
                        break;
                    }
                    default:
                    {
                        log.Error("Card generation failed: An invalid card type was specified.");
                        CardGenerationResponse r = new CardGenerationResponse();
                        r.message = "An invalid card type was specified.";
                        HttpResponseMessage rep =
                            req.CreateResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(r));
                        return rep;

                    }
                }
                CardGenerationResponse response = new CardGenerationResponse();
                response.message = "OK";
                Stream cardtobyte = new MemoryStream();
                card.Save(cardtobyte,ImageFormat.Png);
                byte[] beforebase64 = new byte[cardtobyte.Length];
                cardtobyte.Seek(0, SeekOrigin.Begin);
                cardtobyte.Read(beforebase64, 0, (int)cardtobyte.Length);
                response.card = Convert.ToBase64String(beforebase64);
                HttpResponseMessage outgoing =
                    req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(response));
                return outgoing;
            }
            catch (Exception ex)
            {
                log.Error("Card generation failed: " + ex.Message);
                CardGenerationResponse r = new CardGenerationResponse();
                r.message = "An unknown error occurred.";
                HttpResponseMessage rep =
                    req.CreateResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(r));
                return rep;
            }

        }

        // The following function is adapted from my work at the University of Washington Bothell

        public static Image GenerateHearthstone(HearthstoneCard request)
        {
            // Setup our output card and all necessary fonts
            Image card = new Bitmap(400, 543);
            Graphics canvas = Graphics.FromImage(card);
            canvas.TextRenderingHint = TextRenderingHint.AntiAlias;
            canvas.SmoothingMode = SmoothingMode.HighQuality;
            PrivateFontCollection myFonts = new PrivateFontCollection();
            Stream fontStream = new MemoryStream(Assets.Hearthstone.hearthstone);

            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int)fontStream.Length);
            fontStream.Close();
            unsafe
            {
                fixed (byte* pFontData = fontdata)
                {
                    myFonts.AddMemoryFont((System.IntPtr)pFontData, fontdata.Length);
                }
            }

            switch (request.cardType)
            {
                case HearthstoneCard.CardType.Minion:
                    {
                        //case 1
                        canvas.FillRectangle(Brushes.White, new Rectangle(103, 16, 216, 288)); // fill background for transparent images
                        Image bytetoart = new Bitmap(Image.FromStream(new MemoryStream(Convert.FromBase64String(request.artwork))), new Size(216, 288));
                        canvas.DrawImage(bytetoart, new Point(103, 16));

                        Image backing = null;
                        switch (request.cardClass)
                        {
                            case HearthstoneCard.CardClass.Neutral:
                                backing = Assets.Hearthstone.card_minion_neutral;
                                break;
                            case HearthstoneCard.CardClass.Paladin:
                                backing = Assets.Hearthstone.card_minion_paladin;
                                break;
                            case HearthstoneCard.CardClass.DeathKnight:
                                backing = Assets.Hearthstone.card_minion_dk_kotft;
                                break;
                            case HearthstoneCard.CardClass.Druid:
                                backing = Assets.Hearthstone.card_minion_druid;
                                break;
                            case HearthstoneCard.CardClass.Hunter:
                                backing = Assets.Hearthstone.card_minion_hunter;
                                break;
                            case HearthstoneCard.CardClass.Mage:
                                backing = Assets.Hearthstone.card_minion_mage;
                                break;
                            case HearthstoneCard.CardClass.Monk:
                                backing = Assets.Hearthstone.card_minion_monk;
                                break;
                            case HearthstoneCard.CardClass.Priest:
                                backing = Assets.Hearthstone.card_minion_priest;
                                break;
                            case HearthstoneCard.CardClass.Rogue:
                                backing = Assets.Hearthstone.card_minion_rogue;
                                break;
                            case HearthstoneCard.CardClass.Shaman:
                                backing = Assets.Hearthstone.card_minion_shaman;
                                break;
                            case HearthstoneCard.CardClass.Warlock:
                                backing = Assets.Hearthstone.card_minion_warlock;
                                break;
                            case HearthstoneCard.CardClass.Warrior:
                                backing = Assets.Hearthstone.card_minion_warrior;
                                break;

                        }
                        canvas.DrawImage(backing, new Point(0, 0));
                        if (request.rarity != HearthstoneCard.CardRarity.Free)
                        {
                            canvas.DrawImage(new Bitmap(Assets.Hearthstone.on_card_swirl_basic_minion,153,121), new Point(132, 348));
                        }

                        if (!IsEmpty(request.tribe))
                        {
                            
                            if (request.tribe.Length < 10)
                            {
                                int spacing = 19 - request.tribe.Length;
                                spacing /= 2;
                                for (int i = 0; i < spacing; i++)
                                {
                                    request.tribe = " " + request.tribe;
                                }

                            } else if (request.tribe.Length > 12)
                            {
                                request.tribe = request.tribe.Substring(0, 12);
                            }


                            canvas.DrawImage(Assets.Hearthstone.card_race,new Point(135,468));
                            TextOutline outlinedtext = new TextOutline(request.tribe, 13, myFonts.Families[0], 150, 477);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.tribe, new Font(myFonts.Families[0], 13), Brushes.White, 150.5f, 477);
                        }
                        // draw text
                        if (request.cost.Length > 1)
                        {
                            TextOutline outlinedtext = new TextOutline(request.cost, 56, myFonts.Families[0], 12, 5);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.cost, new Font(myFonts.Families[0], 56), Brushes.White, 12.5f, 5);
                        }
                        else
                        {
                            TextOutline outlinedtext = new TextOutline(request.cost, 72, myFonts.Families[0], 18, -10);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.cost, new Font(myFonts.Families[0], 72), Brushes.White, 18.5f,
                                -10);
                        }

                        if (request.health.Length > 1)
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.health, 56, myFonts.Families[0], 300, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.health, new Font(myFonts.Families[0], 56), Brushes.White, 300.5f,
                                435);
                        }
                        else
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.health, 56, myFonts.Families[0], 320, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.health, new Font(myFonts.Families[0], 56), Brushes.White, 320.5f,
                                435);
                        }

                        if (request.attack.Length > 1)
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.attack, 56, myFonts.Families[0], 12, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.attack, new Font(myFonts.Families[0], 56), Brushes.White, 12.5f,
                                435);
                        }
                        else
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.attack, 56, myFonts.Families[0], 34, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.attack, new Font(myFonts.Families[0], 56), Brushes.White, 34.5f,
                                435);
                        }

                        Rectangle r = new Rectangle(new Point(80, 350), new Size(250, 90));
                        StringFormat s = new StringFormat();
                        s.Alignment = StringAlignment.Center;
                        s.LineAlignment = StringAlignment.Center;
                        //canvas.DrawRectangle(new Pen(Color.Aqua),r );
                        if (request.text.Length > 45)
                        {
                            canvas.DrawString(request.text, new Font(FontFamily.GenericSansSerif, 12),
                                Brushes.Black, r, s);
                        }
                        else
                        {
                            canvas.DrawString(request.text, new Font(FontFamily.GenericSansSerif, 18),
                                Brushes.Black, r, s);
                        }

                        PointF[] titlecurve = new PointF[]
                        {
                            new Point(80, 320), new Point(125, 315), new Point(175, 305), new Point(220, 297),
                            new Point(265, 295), new Point(325, 300)
                        };

                        if (request.cardName.Length <= 14)
                        {
                            int spacing = 19 - request.cardName.Length;
                            if (request.cardName.Length >= 12)
                            {
                                spacing = 17 - request.cardName.Length;
                            }
                            spacing /= 2;
                            for (int i = 0; i < spacing; i++)
                            {
                                request.cardName = " " + request.cardName;
                            }
                        }

                        request.cardName = request.cardName.Replace(" ", "_");
                        TextOutline titletext = new TextOutline(request.cardName, 16, myFonts.Families[0], 34, 435);
                        GraphicsPath path = new GraphicsPath();
                        path.AddCurve(titlecurve);
                        titletext.DrawTextOnPath(true, canvas, path);
                        titletext.DrawTextOnPath(false, canvas, path);

                        if (request.rarity != HearthstoneCard.CardRarity.Free)
                        {
                            canvas.DrawImage(Assets.Hearthstone.minion_gem_brackets,new Point(192,305));
                            Image gem = null;
                            switch (request.rarity)
                            {
                                case HearthstoneCard.CardRarity.Common:
                                    gem = Assets.Hearthstone.gem_common;
                                    break;
                                case HearthstoneCard.CardRarity.Rare:
                                    gem = Assets.Hearthstone.gem_rare;
                                    break;
                                case HearthstoneCard.CardRarity.Epic:
                                    gem = Assets.Hearthstone.gem_epic;
                                    break;
                                case HearthstoneCard.CardRarity.Legendary:
                                    gem = Assets.Hearthstone.gem_legendary;
                                    break;
                                default:
                                    throw new CardGeneratorException("Invalid rarity.");
                            }
                            canvas.DrawImage(gem, new Point(202, 308));
                            if (request.rarity == HearthstoneCard.CardRarity.Legendary)
                            {
                                canvas.Save();
                                Bitmap b = new Bitmap(400,573);
                                canvas = Graphics.FromImage(b);
                                canvas.FillRectangle(Brushes.White,new Rectangle(0,0,400,573));
                                canvas.DrawImage(card,new Point(0,20));
                                canvas.Save();
                                card = b;
                                canvas = Graphics.FromImage(card);
                                //draw dragon
                                Image dragon = new Bitmap(Assets.Hearthstone.card_minion_legendary_dragon_bracket,309,219);
                                canvas.DrawImage(dragon,new Point(95,-5));
                            }
                        }

                        canvas.Save();
                        return card;
                    }
                case HearthstoneCard.CardType.Weapon:
                    {
                        //case 1
                        canvas.FillRectangle(Brushes.White, new Rectangle(83, 16, 246, 288)); // fill background for transparent images
                        Image bytetoart = new Bitmap(Image.FromStream(new MemoryStream(Convert.FromBase64String(request.artwork))), new Size(246, 288));
                        canvas.DrawImage(bytetoart, new Point(83, 16));
                        Image backing = null;
                        switch (request.cardClass)
                        {
                            case HearthstoneCard.CardClass.Neutral:
                                backing = Assets.Hearthstone.card_weapon_neutral;
                                break;
                            case HearthstoneCard.CardClass.Paladin:
                                backing = Assets.Hearthstone.card_weapon_paladin;
                                break;
                            case HearthstoneCard.CardClass.DeathKnight:
                                backing = Assets.Hearthstone.card_weapon_dk_kotft;
                                break;
                            case HearthstoneCard.CardClass.Druid:
                                backing = Assets.Hearthstone.card_weapon_druid;
                                break;
                            case HearthstoneCard.CardClass.Hunter:
                                backing = Assets.Hearthstone.card_weapon_hunter;
                                break;
                            case HearthstoneCard.CardClass.Mage:
                                backing = Assets.Hearthstone.card_weapon_mage;
                                break;
                            case HearthstoneCard.CardClass.Monk:
                                backing = Assets.Hearthstone.card_weapon_monk;
                                break;
                            case HearthstoneCard.CardClass.Priest:
                                backing = Assets.Hearthstone.card_weapon_priest;
                                break;
                            case HearthstoneCard.CardClass.Rogue:
                                backing = Assets.Hearthstone.card_weapon_rogue;
                                break;
                            case HearthstoneCard.CardClass.Shaman:
                                backing = Assets.Hearthstone.card_weapon_shaman;
                                break;
                            case HearthstoneCard.CardClass.Warlock:
                                backing = Assets.Hearthstone.card_weapon_warlock;
                                break;
                            case HearthstoneCard.CardClass.Warrior:
                                backing = Assets.Hearthstone.card_weapon_warrior;
                                break;

                        }
                        canvas.DrawImage(backing, new Point(0, 0));
                        if (request.rarity != HearthstoneCard.CardRarity.Free)
                        {
                            canvas.DrawImage(new Bitmap(Assets.Hearthstone.on_card_swirl_basic_weapon, 172, 140), new Point(127, 348));
                        }
                        // draw text
                        if (request.cost.Length > 1)
                        {
                            TextOutline outlinedtext = new TextOutline(request.cost, 56, myFonts.Families[0], 8, 5);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.cost, new Font(myFonts.Families[0], 56), Brushes.White, 8.5f, 5);
                        }
                        else
                        {
                            TextOutline outlinedtext = new TextOutline(request.cost, 72, myFonts.Families[0], 18, -10);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.cost, new Font(myFonts.Families[0], 72), Brushes.White, 18.5f,
                                -10);
                        }

                        if (request.health.Length > 1)
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.health, 56, myFonts.Families[0], 300, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.health, new Font(myFonts.Families[0], 56), Brushes.White, 300.5f,
                                435);
                        }
                        else
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.health, 56, myFonts.Families[0], 320, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.health, new Font(myFonts.Families[0], 56), Brushes.White, 320.5f,
                                435);
                        }

                        if (request.attack.Length > 1)
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.attack, 56, myFonts.Families[0], 12, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.attack, new Font(myFonts.Families[0], 56), Brushes.White, 12.5f,
                                435);
                        }
                        else
                        {
                            TextOutline outlinedtext =
                                new TextOutline(request.attack, 56, myFonts.Families[0], 34, 435);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.attack, new Font(myFonts.Families[0], 56), Brushes.White, 34.5f,
                                435);
                        }

                        Rectangle r = new Rectangle(new Point(80, 350), new Size(250, 90));
                        StringFormat s = new StringFormat();
                        s.Alignment = StringAlignment.Center;
                        s.LineAlignment = StringAlignment.Center;
                        //canvas.DrawRectangle(new Pen(Color.Aqua),r );
                        if (request.text.Length > 45)
                        {
                            canvas.DrawString(request.text, new Font(FontFamily.GenericSansSerif, 12),
                                Brushes.White, r, s);
                        }
                        else
                        {
                            canvas.DrawString(request.text, new Font(FontFamily.GenericSansSerif, 18),
                                Brushes.White, r, s);
                        }

                        PointF[] titlecurve = new PointF[]
                        {
                            new Point(80, 295), new Point(125, 295), new Point(175, 295), new Point(220, 295),
                            new Point(265, 295), new Point(325, 295)
                        };
                        //foreach (var v in titlecurve)
                        //{
                        //    canvas.DrawRectangle(new Pen(Color.Red), v.X, v.Y, 4, 4);
                        //}
                        //canvas.DrawCurve(new Pen(Color.Aqua), titlecurve);
                        if (request.cardName.Length <= 14)
                        {
                            int spacing = 19 - request.cardName.Length;
                            spacing /= 2;
                            for (int i = 0; i < spacing; i++)
                            {
                                request.cardName = " " + request.cardName;
                            }
                        }

                        request.cardName = request.cardName.Replace(" ", "_");
                        TextOutline titletext = new TextOutline(request.cardName, 16, myFonts.Families[0], 34, 435);
                        GraphicsPath path = new GraphicsPath();
                        path.AddCurve(titlecurve);
                        titletext.DrawTextOnPath(true, canvas, path);
                        titletext.DrawTextOnPath(false, canvas, path);

                        if (request.rarity != HearthstoneCard.CardRarity.Free)
                        {
                            canvas.DrawImage(Assets.Hearthstone.weapon_gem_brackets, new Point(176, 305));
                            Image gem = null;
                            switch (request.rarity)
                            {
                                case HearthstoneCard.CardRarity.Common:
                                    gem = Assets.Hearthstone.gem_common;
                                    break;
                                case HearthstoneCard.CardRarity.Rare:
                                    gem = Assets.Hearthstone.gem_rare;
                                    break;
                                case HearthstoneCard.CardRarity.Epic:
                                    gem = Assets.Hearthstone.gem_epic;
                                    break;
                                case HearthstoneCard.CardRarity.Legendary:
                                    gem = Assets.Hearthstone.gem_legendary;
                                    break;
                                default:
                                    throw new CardGeneratorException("Invalid rarity.");
                            }
                            canvas.DrawImage(gem, new Point(189, 306));
                        }

                        canvas.Save();
                        return card;
                    }
                case HearthstoneCard.CardType.Spell:
                    {
                        //case 1
                        canvas.FillRectangle(Brushes.White, new Rectangle(68, 26, 275, 288)); // fill background for transparent images
                        Image bytetoart = new Bitmap(Image.FromStream(new MemoryStream(Convert.FromBase64String(request.artwork))), new Size(275, 288));
                        canvas.DrawImage(bytetoart, new Point(68, 26));
                        Image backing = null;
                        switch (request.cardClass)
                        {
                            case HearthstoneCard.CardClass.Neutral:
                                backing = Assets.Hearthstone.card_spell_neutral;
                                break;
                            case HearthstoneCard.CardClass.Paladin:
                                backing = Assets.Hearthstone.card_spell_paladin;
                                break;
                            case HearthstoneCard.CardClass.DeathKnight:
                                backing = Assets.Hearthstone.card_spell_dk_kotft;
                                break;
                            case HearthstoneCard.CardClass.Druid:
                                backing = Assets.Hearthstone.card_spell_druid;
                                break;
                            case HearthstoneCard.CardClass.Hunter:
                                backing = Assets.Hearthstone.card_spell_hunter;
                                break;
                            case HearthstoneCard.CardClass.Mage:
                                backing = Assets.Hearthstone.card_spell_mage;
                                break;
                            case HearthstoneCard.CardClass.Monk:
                                backing = Assets.Hearthstone.card_spell_monk;
                                break;
                            case HearthstoneCard.CardClass.Priest:
                                backing = Assets.Hearthstone.card_spell_priest;
                                break;
                            case HearthstoneCard.CardClass.Rogue:
                                backing = Assets.Hearthstone.card_spell_rogue;
                                break;
                            case HearthstoneCard.CardClass.Shaman:
                                backing = Assets.Hearthstone.card_spell_shaman;
                                break;
                            case HearthstoneCard.CardClass.Warlock:
                                backing = Assets.Hearthstone.card_spell_warlock;
                                break;
                            case HearthstoneCard.CardClass.Warrior:
                                backing = Assets.Hearthstone.card_spell_warrior;
                                break;

                        }
                        canvas.DrawImage(backing, new Point(0, 0));
                        if (request.rarity != HearthstoneCard.CardRarity.Free)
                        {
                            canvas.DrawImage(new Bitmap(Assets.Hearthstone.on_card_swirl_basic_spell, 172, 140), new Point(127, 348));
                        }
                        // draw text
                        if (request.cost.Length > 1)
                        {
                            TextOutline outlinedtext = new TextOutline(request.cost, 56, myFonts.Families[0], 12, 5);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.cost, new Font(myFonts.Families[0], 56), Brushes.White, 12.5f, 5);
                        }
                        else
                        {
                            TextOutline outlinedtext = new TextOutline(request.cost, 72, myFonts.Families[0], 18, -10);
                            outlinedtext.Paint(canvas);
                            canvas.DrawString(request.cost, new Font(myFonts.Families[0], 72), Brushes.White, 18.5f,
                                -10);
                        }


                        Rectangle r = new Rectangle(new Point(80, 380), new Size(250, 90));
                        StringFormat s = new StringFormat();
                        s.Alignment = StringAlignment.Center;
                        s.LineAlignment = StringAlignment.Center;
                        //canvas.DrawRectangle(new Pen(Color.Aqua),r );
                        if (request.text.Length > 45)
                        {
                            canvas.DrawString(request.text, new Font(FontFamily.GenericSansSerif, 12),
                                Brushes.Black, r, s);
                        }
                        else
                        {
                            canvas.DrawString(request.text, new Font(FontFamily.GenericSansSerif, 18),
                                Brushes.Black, r, s);
                        }

                        PointF[] titlecurve = new PointF[]
                        {
                            new Point(80, 305), new Point(125, 295), new Point(165, 290), new Point(230, 290),
                            new Point(265, 295), new Point(325, 305)
                        };
                        //foreach (var v in titlecurve)
                        //{
                        //    canvas.DrawRectangle(new Pen(Color.Red), v.X, v.Y, 4, 4);
                        //}
                        //canvas.DrawCurve(new Pen(Color.Aqua), titlecurve);
                        if (request.cardName.Length <= 14)
                        {
                            int spacing = 19 - request.cardName.Length;
                            spacing /= 2;
                            for (int i = 0; i < spacing; i++)
                            {
                                request.cardName = " " + request.cardName;
                            }
                        }

                        request.cardName = request.cardName.Replace(" ", "_");
                        TextOutline titletext = new TextOutline(request.cardName, 16, myFonts.Families[0], 34, 435);
                        GraphicsPath path = new GraphicsPath();
                        path.AddCurve(titlecurve);
                        titletext.DrawTextOnPath(true, canvas, path);
                        titletext.DrawTextOnPath(false, canvas, path);
                        if (request.rarity != HearthstoneCard.CardRarity.Free)
                        {
                            canvas.DrawImage(Assets.Hearthstone.spell_gem_brackets, new Point(169, 301));
                            Image gem = null;
                            switch (request.rarity)
                            {
                                case HearthstoneCard.CardRarity.Common:
                                    gem = Assets.Hearthstone.gem_common;
                                    break;
                                case HearthstoneCard.CardRarity.Rare:
                                    gem = Assets.Hearthstone.gem_rare;
                                    break;
                                case HearthstoneCard.CardRarity.Epic:
                                    gem = Assets.Hearthstone.gem_epic;
                                    break;
                                case HearthstoneCard.CardRarity.Legendary:
                                    gem = Assets.Hearthstone.gem_legendary;
                                    break;
                                default:
                                    throw new CardGeneratorException("Invalid rarity.");
                            }
                            canvas.DrawImage(gem, new Point(188, 311));
                        }
                        canvas.Save();
                        return card;
                    }
                default:
                    {
                        throw new CardGeneratorException("Card type is unsupported.");
                    }
            }
        }

        /// <summary>
        /// Helper method. Checks if the string is empty or consists of only
        /// whitespace.
        /// </summary>
        /// <param name="text">String to check</param>
        /// <returns></returns>
        public static bool IsEmpty(string text)
        {
            // Return false if any of the characters are not whitespace
            return text.All(char.IsWhiteSpace);
        }

        // The following code was adapted from https://stackoverflow.com/questions/4200843/outline-text-with-system-drawing
        // http://csharphelper.com/blog/2016/01/draw-text-on-a-line-segment-in-c/
        // http://csharphelper.com/blog/2016/01/draw-text-on-a-curve-in-c/

        public class TextOutline
        {
            public TextOutline(string text, int size, FontFamily font, int x, int y)
            {
                OutlineForeColor = Color.Black;
                OutlineWidth = 4;
                Text = text;
                Font = font;
                FontSize = size;
                this.x = x;
                this.y = y;

            }
            public Color OutlineForeColor { get; set; }
            public float OutlineWidth { get; set; }
            public string Text { get; set; }
            public int FontSize { get; set; }
            public FontFamily Font { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public void Paint(Graphics g)
            {
                GraphicsPath p = new GraphicsPath();

                p.AddString(
                    Text,             // text to draw
                    Font,  // or any other font family
                    (int)FontStyle.Regular,      // font style (bold, italic, etc.)
                    g.DpiY * FontSize / 72,       // em size
                    new Point(x, y),              // location where to draw text
                    new StringFormat());          // set options here (e.g. center alignment)
                g.DrawPath(new Pen(OutlineForeColor, OutlineWidth), p);
            }

            public void DrawSegment(bool isOutline, Graphics g, ref PointF last, ref PointF first, ref int firstchar)
            {
                float dx = last.X - first.X;
                float dy = last.Y - first.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                dx /= distance;
                dy /= distance;

                int lastchar = firstchar;
                while (lastchar < this.Text.Length)
                {
                    string test = this.Text.Substring(firstchar, lastchar - firstchar + 1);
                    if (g.MeasureString(test, new Font(this.Font, this.FontSize)).Width - 5 > distance)
                    {
                        lastchar--;
                        break;
                    }

                    lastchar++;
                }

                if (lastchar < firstchar) return;
                if (lastchar >= this.Text.Length)
                {
                    lastchar = this.Text.Length - 1;
                }

                string validchars = this.Text.Substring(firstchar, lastchar - firstchar + 1);

                GraphicsState s = g.Save();
                g.TranslateTransform(0, -g.MeasureString(validchars, new Font(this.Font, this.FontSize)).Height, MatrixOrder.Append);
                float angle = (float)(180 * Math.Atan2(dy, dx) / Math.PI);
                g.RotateTransform(angle, MatrixOrder.Append);
                g.TranslateTransform(first.X, first.Y, MatrixOrder.Append);

                Brush mycolor;
                if (validchars.Equals("_"))
                {
                    mycolor = Brushes.Transparent;
                }
                else if (isOutline)
                {
                    mycolor = Brushes.Black;
                    if (validchars.Contains("_"))
                    {
                        validchars = validchars.Replace("_", " ");
                    }
                }
                else
                {
                    mycolor = Brushes.White;
                    if (validchars.Contains("_"))
                    {
                        validchars = validchars.Replace("_", " ");
                    }
                }

                if (isOutline)
                {
                    GraphicsPath p = new GraphicsPath();

                    p.AddString(
                        validchars,             // text to draw
                        Font,  // or any other font family
                        (int)FontStyle.Regular,      // font style (bold, italic, etc.)
                        g.DpiY * FontSize / 72,       // em size
                        new Point(0, 0),              // location where to draw text
                        new StringFormat());          // set options here (e.g. center alignment)
                    g.DrawPath(new Pen(mycolor, OutlineWidth), p);
                }
                else
                {
                    g.DrawString(validchars, new Font(this.Font, this.FontSize), mycolor, 0, 0);
                }
                g.Restore(s);
                firstchar = lastchar + 1;
                float width = g.MeasureString(validchars, new Font(this.Font, this.FontSize)).Width - 5;
                first = new PointF(first.X + dx * width, first.Y + dy * width);
            }

            public void DrawTextOnPath(bool isOutline, Graphics g, GraphicsPath p)
            {
                p = (GraphicsPath)p.Clone();
                p.Flatten();
                int start = 0;
                PointF startpt = p.PathPoints[0];
                for (int i = 1; i < p.PointCount; i++)
                {
                    PointF endpt = p.PathPoints[i];
                    DrawSegment(isOutline, g, ref endpt, ref startpt, ref start);
                    if (start >= this.Text.Length) break;
                }
            }
        }
    }
}
