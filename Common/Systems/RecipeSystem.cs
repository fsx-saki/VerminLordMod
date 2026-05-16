using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Walls;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Placeable;
using VerminLordMod.Content.Items.Placeable.Furniture;
using VerminLordMod.Content.Items.Placeable.Walls;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // RecipeSystem — 配方系统大框
    //
    // TODO:
    //   - 填充所有蛊虫武器配方（一转到六转）
    //   - 填充所有消耗品配方（突破丹药、蛊虫食物、元石相关）
    //   - 填充所有饰品配方（翅膀、蛊甲等）
    //   - 填充所有方块/墙壁配方（建筑类）
    //   - 填充所有家具配方（青茅石家具系列）
    //   - 填充特殊配方（阵法道具、炼器材料等）
    // TODO 使用更简洁的配方注册方式
    // ============================================================
    public class RecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            AddBuildingRecipes();
            AddOneTurnGuRecipes();
            AddTwoTurnGuRecipes();
            AddThreeTurnGuRecipes();
            AddConsumableRecipes();
            AddAccessoryRecipes();
            AddFurnitureRecipes();
        }

        private void AddBuildingRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var guYueWood = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.GuYueWoodBlock>(), 10);
            guYueWood.AddIngredient(ItemID.Wood, 10);
            guYueWood.AddIngredient(yuanS, 1);
            guYueWood.AddTile(TileID.WorkBenches);
            guYueWood.Register();

            var guYuePlatform = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.GuYueBambooPlatform>(), 10);
            guYuePlatform.AddIngredient(ItemID.Wood, 10);
            guYuePlatform.AddIngredient(ItemID.BambooBlock, 5);
            guYuePlatform.AddTile(TileID.WorkBenches);
            guYuePlatform.Register();

            var guYueWall = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.GuYueBambooWall>(), 10);
            guYueWall.AddIngredient(ItemID.Wood, 10);
            guYueWall.AddIngredient(ItemID.BambooBlock, 5);
            guYueWall.AddTile(TileID.WorkBenches);
            guYueWall.Register();

            var baiJade = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.BaiJadeBlock>(), 10);
            baiJade.AddIngredient(ItemID.StoneBlock, 10);
            baiJade.AddIngredient(ItemID.IceBlock, 5);
            baiJade.AddIngredient(yuanS, 1);
            baiJade.AddTile(TileID.WorkBenches);
            baiJade.Register();

            var baiSilver = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.BaiSilverBrick>(), 10);
            baiSilver.AddIngredient(ItemID.StoneBlock, 10);
            baiSilver.AddIngredient(ItemID.SilverBar, 1);
            baiSilver.AddTile(TileID.WorkBenches);
            baiSilver.Register();

            var baiPillar = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.BaiJadePillar>(), 10);
            baiPillar.AddIngredient(ItemID.StoneBlock, 10);
            baiPillar.AddIngredient(ItemID.IceBlock, 5);
            baiPillar.AddIngredient(yuanS, 1);
            baiPillar.AddTile(TileID.WorkBenches);
            baiPillar.Register();

            var xiongStone = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.XiongDarkStone>(), 10);
            xiongStone.AddIngredient(ItemID.StoneBlock, 10);
            xiongStone.AddIngredient(ItemID.Bone, 5);
            xiongStone.AddIngredient(yuanS, 1);
            xiongStone.AddTile(TileID.WorkBenches);
            xiongStone.Register();

            var xiongBeam = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.XiongIronBeam>(), 10);
            xiongBeam.AddIngredient(ItemID.IronBar, 2);
            xiongBeam.AddIngredient(ItemID.Bone, 5);
            xiongBeam.AddTile(TileID.Anvils);
            xiongBeam.Register();

            var tieStone = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.TieForgeStone>(), 10);
            tieStone.AddIngredient(ItemID.StoneBlock, 10);
            tieStone.AddIngredient(ItemID.AshBlock, 5);
            tieStone.AddIngredient(yuanS, 1);
            tieStone.AddTile(TileID.WorkBenches);
            tieStone.Register();

            var tieWall = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.TieForgeWall>(), 10);
            tieWall.AddIngredient(ItemID.StoneBlock, 10);
            tieWall.AddIngredient(ItemID.AshBlock, 5);
            tieWall.AddTile(TileID.WorkBenches);
            tieWall.Register();

            var wangCrystal = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.WangCrystalBlock>(), 10);
            wangCrystal.AddIngredient(ItemID.Glass, 10);
            wangCrystal.AddIngredient(ItemID.CrystalShard, 1);
            wangCrystal.AddIngredient(yuanS, 1);
            wangCrystal.AddTile(TileID.WorkBenches);
            wangCrystal.Register();

            var wangPillar = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.WangCrystalPillar>(), 10);
            wangPillar.AddIngredient(ItemID.Glass, 10);
            wangPillar.AddIngredient(ItemID.CrystalShard, 1);
            wangPillar.AddIngredient(yuanS, 1);
            wangPillar.AddTile(TileID.WorkBenches);
            wangPillar.Register();

            var zhaoBrick = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ZhaoShadowBrick>(), 10);
            zhaoBrick.AddIngredient(ItemID.StoneBlock, 10);
            zhaoBrick.AddIngredient(ItemID.Obsidian, 5);
            zhaoBrick.AddIngredient(yuanS, 1);
            zhaoBrick.AddTile(TileID.WorkBenches);
            zhaoBrick.Register();

            var zhaoCurtain = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ZhaoShadowCurtain>(), 10);
            zhaoCurtain.AddIngredient(ItemID.Silk, 5);
            zhaoCurtain.AddIngredient(ItemID.Obsidian, 3);
            zhaoCurtain.AddTile(TileID.WorkBenches);
            zhaoCurtain.Register();

            var jiaSilk = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.JiaGoldSilkBlock>(), 10);
            jiaSilk.AddIngredient(ItemID.Silk, 10);
            jiaSilk.AddIngredient(ItemID.GoldBar, 1);
            jiaSilk.AddIngredient(yuanS, 1);
            jiaSilk.AddTile(TileID.WorkBenches);
            jiaSilk.Register();

            var jiaCurtain = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.JiaSilkCurtain>(), 10);
            jiaCurtain.AddIngredient(ItemID.Silk, 10);
            jiaCurtain.AddIngredient(ItemID.GoldBar, 1);
            jiaCurtain.AddTile(TileID.WorkBenches);
            jiaCurtain.Register();

            var scatteredBrick = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ScatteredMudBrick>(), 10);
            scatteredBrick.AddIngredient(ItemID.MudBlock, 10);
            scatteredBrick.AddIngredient(ItemID.ClayBlock, 5);
            scatteredBrick.AddTile(TileID.WorkBenches);
            scatteredBrick.Register();
        }

        private void AddOneTurnGuRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var moonlight = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.Moonlight>());
            moonlight.AddIngredient(yuanS, 5);
            moonlight.AddIngredient(ItemID.FallenStar, 3);
            moonlight.AddTile(TileID.WorkBenches);
            moonlight.Register();

            var boneSpear = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.BoneSpearGu>());
            boneSpear.AddIngredient(yuanS, 5);
            boneSpear.AddIngredient(ItemID.Bone, 10);
            boneSpear.AddTile(TileID.WorkBenches);
            boneSpear.Register();

            var waterArrow = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.WaterArrowGu>());
            waterArrow.AddIngredient(yuanS, 3);
            waterArrow.AddIngredient(ItemID.Coral, 5);
            waterArrow.AddTile(TileID.WorkBenches);
            waterArrow.Register();

            var grassPuppet = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.GrassPuppet>());
            grassPuppet.AddIngredient(yuanS, 5);
            grassPuppet.AddIngredient(ItemID.Vine, 5);
            grassPuppet.AddIngredient(ItemID.Daybloom, 3);
            grassPuppet.AddTile(TileID.WorkBenches);
            grassPuppet.Register();

            var dirtGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.DirtGu>());
            dirtGu.AddIngredient(yuanS, 3);
            dirtGu.AddIngredient(ItemID.DirtBlock, 20);
            dirtGu.AddTile(TileID.WorkBenches);
            dirtGu.Register();

            var starDart = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.StarDartGu>());
            starDart.AddIngredient(yuanS, 5);
            starDart.AddIngredient(ItemID.FallenStar, 5);
            starDart.AddTile(TileID.WorkBenches);
            starDart.Register();

            var meteorGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.MeteorGu>());
            meteorGu.AddIngredient(yuanS, 8);
            meteorGu.AddIngredient(ItemID.Meteorite, 15);
            meteorGu.AddTile(TileID.Anvils);
            meteorGu.Register();

            var liquidFlame = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.LiquidFlameGu>());
            liquidFlame.AddIngredient(yuanS, 5);
            liquidFlame.AddIngredient(ItemID.Gel, 15);
            liquidFlame.AddIngredient(ItemID.Torch, 5);
            liquidFlame.AddTile(TileID.WorkBenches);
            liquidFlame.Register();

            var breezeWheel = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.BreezeWheelGu>());
            breezeWheel.AddIngredient(yuanS, 5);
            breezeWheel.AddIngredient(ItemID.Feather, 5);
            breezeWheel.AddTile(TileID.WorkBenches);
            breezeWheel.Register();

            var fireClothes = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.FireClothesGu>());
            fireClothes.AddIngredient(yuanS, 5);
            fireClothes.AddIngredient(ItemID.Silk, 10);
            fireClothes.AddIngredient(ItemID.Torch, 3);
            fireClothes.AddTile(TileID.Loom);
            fireClothes.Register();

            var littleSoul = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.LittleSoulGu>());
            littleSoul.AddIngredient(yuanS, 5);
            littleSoul.AddIngredient(ItemID.Bone, 15);
            littleSoul.AddTile(TileID.WorkBenches);
            littleSoul.Register();

            var riceBag = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.RiceBagGrassGu>());
            riceBag.AddIngredient(yuanS, 3);
            riceBag.AddIngredient(ItemID.Hay, 20);
            riceBag.AddTile(TileID.WorkBenches);
            riceBag.Register();

            var starRiver = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.StarRiverGu>());
            starRiver.AddIngredient(yuanS, 8);
            starRiver.AddIngredient(ItemID.FallenStar, 8);
            starRiver.AddTile(TileID.WorkBenches);
            starRiver.Register();

            var warmSpider = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.WarmStringSpider>());
            warmSpider.AddIngredient(yuanS, 5);
            warmSpider.AddIngredient(ItemID.Cobweb, 15);
            warmSpider.AddTile(TileID.WorkBenches);
            warmSpider.Register();

            var stoveGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.StoveGu>());
            stoveGu.AddIngredient(yuanS, 5);
            stoveGu.AddIngredient(ItemID.StoneBlock, 20);
            stoveGu.AddIngredient(ItemID.Torch, 5);
            stoveGu.AddTile(TileID.WorkBenches);
            stoveGu.Register();

            var wineBug = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.WineBugWeapon>());
            wineBug.AddIngredient(yuanS, 5);
            wineBug.AddIngredient(ItemID.Mushroom, 10);
            wineBug.AddTile(TileID.WorkBenches);
            wineBug.Register();

            var wineBagFlower = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.WineBagFlowerGu>());
            wineBagFlower.AddIngredient(yuanS, 5);
            wineBagFlower.AddIngredient(ItemID.Daybloom, 5);
            wineBagFlower.AddIngredient(ItemID.Mushroom, 5);
            wineBagFlower.AddTile(TileID.WorkBenches);
            wineBagFlower.Register();

            var blackHair = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.BlackHair>());
            blackHair.AddIngredient(yuanS, 5);
            blackHair.AddIngredient(ItemID.Cobweb, 10);
            blackHair.AddTile(TileID.WorkBenches);
            blackHair.Register();

            var skyMeteor = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.SkyMeteorGu>());
            skyMeteor.AddIngredient(yuanS, 8);
            skyMeteor.AddIngredient(ItemID.Meteorite, 10);
            skyMeteor.AddIngredient(ItemID.FallenStar, 5);
            skyMeteor.AddTile(TileID.Anvils);
            skyMeteor.Register();

            var livingGrass = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.LivingGrass>());
            livingGrass.AddIngredient(yuanS, 5);
            livingGrass.AddIngredient(ItemID.Daybloom, 5);
            livingGrass.AddIngredient(ItemID.Vine, 5);
            livingGrass.AddTile(TileID.WorkBenches);
            livingGrass.Register();

            var minilight = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.Minilight>());
            minilight.AddIngredient(yuanS, 5);
            minilight.AddIngredient(ItemID.GlowingMushroom, 10);
            minilight.AddTile(TileID.WorkBenches);
            minilight.Register();

            var spiritSaliva = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.SpiritSalivaGu>());
            spiritSaliva.AddIngredient(yuanS, 5);
            spiritSaliva.AddIngredient(ItemID.Gel, 10);
            spiritSaliva.AddIngredient(ItemID.Waterleaf, 3);
            spiritSaliva.AddTile(TileID.WorkBenches);
            spiritSaliva.Register();
        }

        private void AddTwoTurnGuRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var shiningGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.ShiningGu>());
            shiningGu.AddIngredient(yuanS, 10);
            shiningGu.AddIngredient(ItemID.FallenStar, 5);
            shiningGu.AddIngredient(ItemID.CrystalShard, 3);
            shiningGu.AddTile(TileID.Anvils);
            shiningGu.Register();

            var moonlightPro = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.MoonlightPro>());
            moonlightPro.AddIngredient(yuanS, 10);
            moonlightPro.AddIngredient(ItemID.FallenStar, 8);
            moonlightPro.AddIngredient(ItemID.SoulofLight, 3);
            moonlightPro.AddTile(TileID.Anvils);
            moonlightPro.Register();

            var loveToLeave = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.LoveToLeave>());
            loveToLeave.AddIngredient(yuanS, 10);
            loveToLeave.AddIngredient(ItemID.LifeCrystal, 1);
            loveToLeave.AddIngredient(ItemID.Daybloom, 5);
            loveToLeave.AddTile(TileID.Anvils);
            loveToLeave.Register();

            var bloodQi = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.BloodQiGu>());
            bloodQi.AddIngredient(yuanS, 10);
            bloodQi.AddIngredient(ItemID.Vertebrae, 10);
            bloodQi.AddTile(TileID.Anvils);
            bloodQi.Register();

            var whiteJade = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.WhiteJadeGu>());
            whiteJade.AddIngredient(yuanS, 10);
            whiteJade.AddIngredient(ItemID.Marble, 15);
            whiteJade.AddTile(TileID.Anvils);
            whiteJade.Register();

            var lightningSpear = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.LightningSpearGu>());
            lightningSpear.AddIngredient(yuanS, 12);
            lightningSpear.AddIngredient(ItemID.Cloud, 15);
            lightningSpear.AddIngredient(ItemID.FallenStar, 5);
            lightningSpear.AddTile(TileID.Anvils);
            lightningSpear.Register();

            var waterDrill = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.WaterDrillGu>());
            waterDrill.AddIngredient(yuanS, 10);
            waterDrill.AddIngredient(ItemID.Coral, 10);
            waterDrill.AddIngredient(ItemID.Seashell, 5);
            waterDrill.AddTile(TileID.Anvils);
            waterDrill.Register();

            var ghostFire = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.GhostFireGu>());
            ghostFire.AddIngredient(yuanS, 10);
            ghostFire.AddIngredient(ItemID.Bone, 20);
            ghostFire.AddIngredient(ItemID.Gel, 10);
            ghostFire.AddTile(TileID.Anvils);
            ghostFire.Register();

            var goldNeedle = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.GoldNeedleGu>());
            goldNeedle.AddIngredient(yuanS, 10);
            goldNeedle.AddIngredient(ItemID.GoldBar, 5);
            goldNeedle.AddTile(TileID.Anvils);
            goldNeedle.Register();

            var spiralBone = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.SpiralBoneSpearGu>());
            spiralBone.AddIngredient(yuanS, 10);
            spiralBone.AddIngredient(ItemID.Bone, 25);
            spiralBone.AddTile(TileID.Anvils);
            spiralBone.Register();

            var iceAwl = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.IceAwlGu>());
            iceAwl.AddIngredient(yuanS, 10);
            iceAwl.AddIngredient(ItemID.IceBlock, 20);
            iceAwl.AddTile(TileID.Anvils);
            iceAwl.Register();

            var iceKnife = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.IceKnifeGu>());
            iceKnife.AddIngredient(yuanS, 10);
            iceKnife.AddIngredient(ItemID.IceBlock, 15);
            iceKnife.AddIngredient(ItemID.Shiverthorn, 3);
            iceKnife.AddTile(TileID.Anvils);
            iceKnife.Register();

            var poisonNeedle = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.PoisonNeedleGu>());
            poisonNeedle.AddIngredient(yuanS, 10);
            poisonNeedle.AddIngredient(ItemID.Stinger, 10);
            poisonNeedle.AddIngredient(ItemID.JungleSpores, 5);
            poisonNeedle.AddTile(TileID.Anvils);
            poisonNeedle.Register();

            var acidWater = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.AcidWaterGu>());
            acidWater.AddIngredient(yuanS, 10);
            acidWater.AddIngredient(ItemID.BottledWater, 5);
            acidWater.AddIngredient(ItemID.Stinger, 5);
            acidWater.AddTile(TileID.Bottles);
            acidWater.Register();

            var rockGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.RockGu>());
            rockGu.AddIngredient(yuanS, 10);
            rockGu.AddIngredient(ItemID.StoneBlock, 30);
            rockGu.AddTile(TileID.Anvils);
            rockGu.Register();

            var plasmaGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.PlasmaGu>());
            plasmaGu.AddIngredient(yuanS, 12);
            plasmaGu.AddIngredient(ItemID.Gel, 20);
            plasmaGu.AddIngredient(ItemID.FallenStar, 3);
            plasmaGu.AddTile(TileID.Anvils);
            plasmaGu.Register();

            var bigSoul = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.BigSoulGu>());
            bigSoul.AddIngredient(yuanS, 10);
            bigSoul.AddIngredient(ItemID.Bone, 20);
            bigSoul.AddIngredient(ItemID.SoulofNight, 3);
            bigSoul.AddTile(TileID.Anvils);
            bigSoul.Register();

            var blackMane = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.BlackMane>());
            blackMane.AddIngredient(yuanS, 10);
            blackMane.AddIngredient(ItemID.RottenChunk, 15);
            blackMane.AddTile(TileID.Anvils);
            blackMane.Register();

            var bigStrength = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.BigStrengthGu>());
            bigStrength.AddIngredient(yuanS, 10);
            bigStrength.AddIngredient(ItemID.IronBar, 8);
            bigStrength.AddTile(TileID.Anvils);
            bigStrength.Register();

            var pineNeedle = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Two.PineNeedleGu>());
            pineNeedle.AddIngredient(yuanS, 10);
            pineNeedle.AddIngredient(ItemID.Wood, 20);
            pineNeedle.AddIngredient(ItemID.Acorn, 5);
            pineNeedle.AddTile(TileID.Anvils);
            pineNeedle.Register();
        }

        private void AddThreeTurnGuRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var moonPoison = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.MoonPoisonGu>());
            moonPoison.AddIngredient(yuanS, 20);
            moonPoison.AddIngredient(ItemID.SoulofNight, 5);
            moonPoison.AddIngredient(ItemID.Stinger, 10);
            moonPoison.AddTile(TileID.MythrilAnvil);
            moonPoison.Register();

            var fireHeart = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.FireHeartGu>());
            fireHeart.AddIngredient(yuanS, 20);
            fireHeart.AddIngredient(ItemID.HellstoneBar, 10);
            fireHeart.AddIngredient(ItemID.Fireblossom, 5);
            fireHeart.AddTile(TileID.MythrilAnvil);
            fireHeart.Register();

            var knifeLight = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.KnifeLightGu>());
            knifeLight.AddIngredient(yuanS, 20);
            knifeLight.AddIngredient(ItemID.SoulofLight, 5);
            knifeLight.AddIngredient(ItemID.CrystalShard, 5);
            knifeLight.AddTile(TileID.MythrilAnvil);
            knifeLight.Register();

            var waterShell = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.WaterShellGu>());
            waterShell.AddIngredient(yuanS, 20);
            waterShell.AddIngredient(ItemID.Coral, 15);
            waterShell.AddIngredient(ItemID.Seashell, 10);
            waterShell.AddTile(TileID.MythrilAnvil);
            waterShell.Register();

            var gratingGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.GratingGu>());
            gratingGu.AddIngredient(yuanS, 20);
            gratingGu.AddIngredient(ItemID.SoulofNight, 5);
            gratingGu.AddIngredient(ItemID.Bone, 30);
            gratingGu.AddTile(TileID.MythrilAnvil);
            gratingGu.Register();

            var soundbyte = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.SoundbyteGu>());
            soundbyte.AddIngredient(yuanS, 20);
            soundbyte.AddIngredient(ItemID.SoulofLight, 5);
            soundbyte.AddIngredient(ItemID.Harp, 1);
            soundbyte.AddTile(TileID.MythrilAnvil);
            soundbyte.Register();

            var bloodMoon = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.BloodMoonGu>());
            bloodMoon.AddIngredient(yuanS, 20);
            bloodMoon.AddIngredient(ItemID.Vertebrae, 20);
            bloodMoon.AddIngredient(ItemID.SoulofNight, 5);
            bloodMoon.AddTile(TileID.MythrilAnvil);
            bloodMoon.Register();

            var thunderBall = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.ThunderBallGu>());
            thunderBall.AddIngredient(yuanS, 20);
            thunderBall.AddIngredient(ItemID.Cloud, 20);
            thunderBall.AddIngredient(ItemID.SoulofFlight, 5);
            thunderBall.AddTile(TileID.MythrilAnvil);
            thunderBall.Register();

            var goldMoon = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.GoldMoon>());
            goldMoon.AddIngredient(yuanS, 20);
            goldMoon.AddIngredient(ItemID.GoldBar, 10);
            goldMoon.AddIngredient(ItemID.SoulofLight, 5);
            goldMoon.AddTile(TileID.MythrilAnvil);
            goldMoon.Register();

            var swordShadow = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.SwordShadowGu>());
            swordShadow.AddIngredient(yuanS, 20);
            swordShadow.AddIngredient(ItemID.SoulofNight, 8);
            swordShadow.AddIngredient(ItemID.IronBar, 10);
            swordShadow.AddTile(TileID.MythrilAnvil);
            swordShadow.Register();

            var iceCrystal = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.IceCrystalGu>());
            iceCrystal.AddIngredient(yuanS, 20);
            iceCrystal.AddIngredient(ItemID.IceBlock, 30);
            iceCrystal.AddIngredient(ItemID.SoulofLight, 5);
            iceCrystal.AddTile(TileID.MythrilAnvil);
            iceCrystal.Register();

            var spoutGu = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.SpoutGu>());
            spoutGu.AddIngredient(yuanS, 20);
            spoutGu.AddIngredient(ItemID.BottledWater, 10);
            spoutGu.AddIngredient(ItemID.Coral, 10);
            spoutGu.AddTile(TileID.MythrilAnvil);
            spoutGu.Register();

            var ghostFirePro = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.GhostFireGuPro>());
            ghostFirePro.AddIngredient(yuanS, 20);
            ghostFirePro.AddIngredient(ItemID.Bone, 30);
            ghostFirePro.AddIngredient(ItemID.SoulofNight, 5);
            ghostFirePro.AddTile(TileID.MythrilAnvil);
            ghostFirePro.Register();

            var sawtooth = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.SawtoothGoldenWuGu>());
            sawtooth.AddIngredient(yuanS, 20);
            sawtooth.AddIngredient(ItemID.GoldBar, 15);
            sawtooth.AddIngredient(ItemID.SoulofNight, 5);
            sawtooth.AddTile(TileID.MythrilAnvil);
            sawtooth.Register();

            var eternalLife = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.Three.EternalLifeGu>());
            eternalLife.AddIngredient(yuanS, 25);
            eternalLife.AddIngredient(ItemID.LifeCrystal, 2);
            eternalLife.AddIngredient(ItemID.SoulofLight, 5);
            eternalLife.AddTile(TileID.MythrilAnvil);
            eternalLife.Register();
        }

        private void AddConsumableRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var breakPill1 = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.FirstToSecond>());
            breakPill1.AddIngredient(yuanS, 10);
            breakPill1.AddIngredient(ItemID.Daybloom, 3);
            breakPill1.AddTile(TileID.Bottles);
            breakPill1.Register();

            var breakPill2 = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.SecondToThird>());
            breakPill2.AddIngredient(yuanS, 20);
            breakPill2.AddIngredient(ItemID.Daybloom, 5);
            breakPill2.AddIngredient(ItemID.Moonglow, 3);
            breakPill2.AddTile(TileID.Bottles);
            breakPill2.Register();

            var guFood = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.WineBug>());
            guFood.AddIngredient(ItemID.Gel, 10);
            guFood.AddIngredient(ItemID.Mushroom, 5);
            guFood.AddTile(TileID.WorkBenches);
            guFood.Register();

            var healingPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.HealingPill>(), 3);
            healingPill.AddIngredient(yuanS, 3);
            healingPill.AddIngredient(ItemID.Daybloom, 2);
            healingPill.AddIngredient(ItemID.Mushroom, 2);
            healingPill.AddTile(TileID.Bottles);
            healingPill.Register();

            var qiRecoveryPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.QiRecoveryPill>(), 3);
            qiRecoveryPill.AddIngredient(yuanS, 5);
            qiRecoveryPill.AddIngredient(ItemID.Moonglow, 2);
            qiRecoveryPill.AddIngredient(ItemID.FallenStar, 1);
            qiRecoveryPill.AddTile(TileID.Bottles);
            qiRecoveryPill.Register();

            var detoxPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.DetoxPill>(), 3);
            detoxPill.AddIngredient(yuanS, 5);
            detoxPill.AddIngredient(ItemID.Blinkroot, 2);
            detoxPill.AddIngredient(ItemID.BottledWater, 1);
            detoxPill.AddTile(TileID.Bottles);
            detoxPill.Register();

            var defensePill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.DefensePill>(), 3);
            defensePill.AddIngredient(yuanS, 5);
            defensePill.AddIngredient(ItemID.IronOre, 3);
            defensePill.AddIngredient(ItemID.Daybloom, 2);
            defensePill.AddTile(TileID.Bottles);
            defensePill.Register();

            var strengthPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.StrengthPill>(), 3);
            strengthPill.AddIngredient(yuanS, 5);
            strengthPill.AddIngredient(ItemID.Bone, 5);
            strengthPill.AddIngredient(ItemID.Deathweed, 2);
            strengthPill.AddTile(TileID.Bottles);
            strengthPill.Register();

            var speedPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.SpeedPill>(), 3);
            speedPill.AddIngredient(yuanS, 5);
            speedPill.AddIngredient(ItemID.Feather, 3);
            speedPill.AddIngredient(ItemID.Blinkroot, 2);
            speedPill.AddTile(TileID.Bottles);
            speedPill.Register();

            var perceptionPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.PerceptionPill>(), 3);
            perceptionPill.AddIngredient(yuanS, 8);
            perceptionPill.AddIngredient(ItemID.Lens, 2);
            perceptionPill.AddIngredient(ItemID.Moonglow, 3);
            perceptionPill.AddTile(TileID.Bottles);
            perceptionPill.Register();

            var visionPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.VisionPill>(), 3);
            visionPill.AddIngredient(yuanS, 8);
            visionPill.AddIngredient(ItemID.BlackLens, 1);
            visionPill.AddIngredient(ItemID.Fireblossom, 3);
            visionPill.AddTile(TileID.Bottles);
            visionPill.Register();

            var awakeningPill = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.AwakeningPill>());
            awakeningPill.AddIngredient(yuanS, 30);
            awakeningPill.AddIngredient(ItemID.SoulofLight, 5);
            awakeningPill.AddIngredient(ItemID.SoulofNight, 5);
            awakeningPill.AddIngredient(ItemID.Daybloom, 5);
            awakeningPill.AddTile(TileID.Bottles);
            awakeningPill.Register();

            var thirdToForth = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.ThirdToForth>());
            thirdToForth.AddIngredient(yuanS, 40);
            thirdToForth.AddIngredient(ItemID.SoulofLight, 5);
            thirdToForth.AddIngredient(ItemID.CrystalShard, 5);
            thirdToForth.AddTile(TileID.Bottles);
            thirdToForth.Register();

            var forthToFifth = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.ForthToFifth>());
            forthToFifth.AddIngredient(yuanS, 60);
            forthToFifth.AddIngredient(ItemID.SoulofLight, 10);
            forthToFifth.AddIngredient(ItemID.SoulofNight, 10);
            forthToFifth.AddTile(TileID.Bottles);
            forthToFifth.Register();

            var fifthToSixth = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.FifthToSixth>());
            fifthToSixth.AddIngredient(yuanS, 100);
            fifthToSixth.AddIngredient(ItemID.SoulofLight, 15);
            fifthToSixth.AddIngredient(ItemID.SoulofNight, 15);
            fifthToSixth.AddIngredient(ItemID.Ectoplasm, 5);
            fifthToSixth.AddTile(TileID.Bottles);
            fifthToSixth.Register();
        }

        private void AddAccessoryRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var ironSkin = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.IronSkin>());
            ironSkin.AddIngredient(yuanS, 5);
            ironSkin.AddIngredient(ItemID.IronBar, 8);
            ironSkin.AddTile(TileID.Anvils);
            ironSkin.Register();

            var stoneSkin = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.StoneSkin>());
            stoneSkin.AddIngredient(yuanS, 5);
            stoneSkin.AddIngredient(ItemID.StoneBlock, 20);
            stoneSkin.AddTile(TileID.Anvils);
            stoneSkin.Register();

            var copperSkin = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.CopperSkin>());
            copperSkin.AddIngredient(yuanS, 5);
            copperSkin.AddIngredient(ItemID.CopperBar, 8);
            copperSkin.AddTile(TileID.Anvils);
            copperSkin.Register();

            var jadeSkin = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.JadeSkin>());
            jadeSkin.AddIngredient(yuanS, 8);
            jadeSkin.AddIngredient(ItemID.Emerald, 3);
            jadeSkin.AddIngredient(ItemID.StoneBlock, 15);
            jadeSkin.AddTile(TileID.Anvils);
            jadeSkin.Register();

            var bearPower = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.BearPower>());
            bearPower.AddIngredient(yuanS, 8);
            bearPower.AddIngredient(ItemID.Leather, 5);
            bearPower.AddIngredient(ItemID.Bone, 10);
            bearPower.AddTile(TileID.Anvils);
            bearPower.Register();

            var scaleGu = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.ScaleGu>());
            scaleGu.AddIngredient(yuanS, 5);
            scaleGu.AddIngredient(ItemID.SharkFin, 3);
            scaleGu.AddTile(TileID.Anvils);
            scaleGu.Register();

            var invisibleStone = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.One.InvisibleStoneGu>());
            invisibleStone.AddIngredient(yuanS, 8);
            invisibleStone.AddIngredient(ItemID.Diamond, 1);
            invisibleStone.AddIngredient(ItemID.Blinkroot, 3);
            invisibleStone.AddTile(TileID.Anvils);
            invisibleStone.Register();

            var copperSkinS = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Two.CopperSkinS>());
            copperSkinS.AddIngredient(yuanS, 10);
            copperSkinS.AddIngredient(ItemID.SilverBar, 8);
            copperSkinS.AddTile(TileID.Anvils);
            copperSkinS.Register();

            var invisibleScale = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Two.InvisibleScaleGu>());
            invisibleScale.AddIngredient(yuanS, 10);
            invisibleScale.AddIngredient(ItemID.SharkFin, 5);
            invisibleScale.AddIngredient(ItemID.SoulofLight, 3);
            invisibleScale.AddTile(TileID.Anvils);
            invisibleScale.Register();

            var copperSkinSS = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Three.CopperSkinSS>());
            copperSkinSS.AddIngredient(yuanS, 15);
            copperSkinSS.AddIngredient(ItemID.GoldBar, 8);
            copperSkinSS.AddTile(TileID.MythrilAnvil);
            copperSkinSS.Register();

            var eagleWing = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Three.EagleWingGu>());
            eagleWing.AddIngredient(yuanS, 20);
            eagleWing.AddIngredient(ItemID.SoulofFlight, 15);
            eagleWing.AddIngredient(ItemID.Feather, 10);
            eagleWing.AddTile(TileID.MythrilAnvil);
            eagleWing.Register();

            var thunderWings = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Three.ThunderWings>());
            thunderWings.AddIngredient(yuanS, 20);
            thunderWings.AddIngredient(ItemID.SoulofFlight, 15);
            thunderWings.AddIngredient(ItemID.Cloud, 15);
            thunderWings.AddTile(TileID.MythrilAnvil);
            thunderWings.Register();

            var boneWings = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Four.BoneWings>());
            boneWings.AddIngredient(yuanS, 30);
            boneWings.AddIngredient(ItemID.SoulofFlight, 20);
            boneWings.AddIngredient(ItemID.Bone, 30);
            boneWings.AddTile(TileID.MythrilAnvil);
            boneWings.Register();

            var tengYunWings = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Four.TengYunWings>());
            tengYunWings.AddIngredient(yuanS, 30);
            tengYunWings.AddIngredient(ItemID.SoulofFlight, 20);
            tengYunWings.AddIngredient(ItemID.Cloud, 20);
            tengYunWings.AddTile(TileID.MythrilAnvil);
            tengYunWings.Register();

            var goldenCloak = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Four.GoldenThreadCloakGu>());
            goldenCloak.AddIngredient(yuanS, 25);
            goldenCloak.AddIngredient(ItemID.Silk, 15);
            goldenCloak.AddIngredient(ItemID.GoldBar, 5);
            goldenCloak.AddTile(TileID.MythrilAnvil);
            goldenCloak.Register();

            var toilGu = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Four.ToilGu>());
            toilGu.AddIngredient(yuanS, 25);
            toilGu.AddIngredient(ItemID.SoulofNight, 10);
            toilGu.AddIngredient(ItemID.Silk, 10);
            toilGu.AddTile(TileID.MythrilAnvil);
            toilGu.Register();

            var eagleYang = Recipe.Create(ModContent.ItemType<Content.Items.Accessories.Four.EagleYangGu>());
            eagleYang.AddIngredient(yuanS, 30);
            eagleYang.AddIngredient(ItemID.SoulofFlight, 20);
            eagleYang.AddIngredient(ItemID.SoulofLight, 10);
            eagleYang.AddTile(TileID.MythrilAnvil);
            eagleYang.Register();
        }

        private void AddFurnitureRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();
            var qingMaoBlock = ModContent.ItemType<Content.Items.Placeable.BoneBanbooBlock>();

            var qingMaoTable = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.Furniture.QingMaoStoneTable>());
            qingMaoTable.AddIngredient(qingMaoBlock, 8);
            qingMaoTable.AddTile(TileID.WorkBenches);
            qingMaoTable.Register();

            var qingMaoChair = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.Furniture.QingMaoStoneChair>());
            qingMaoChair.AddIngredient(qingMaoBlock, 4);
            qingMaoChair.AddTile(TileID.WorkBenches);
            qingMaoChair.Register();

            var qingMaoWorkbench = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.Furniture.QingMaoStoneWorkbench>());
            qingMaoWorkbench.AddIngredient(qingMaoBlock, 10);
            qingMaoWorkbench.AddTile(TileID.WorkBenches);
            qingMaoWorkbench.Register();

            var guYueAltar = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.GuYueAltar>());
            guYueAltar.AddIngredient(ItemID.Wood, 20);
            guYueAltar.AddIngredient(yuanS, 5);
            guYueAltar.AddTile(TileID.WorkBenches);
            guYueAltar.Register();

            var guYuePot = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.GuYueMedicinePot>());
            guYuePot.AddIngredient(ItemID.ClayPot, 1);
            guYuePot.AddIngredient(yuanS, 3);
            guYuePot.AddTile(TileID.WorkBenches);
            guYuePot.Register();

            var baiTable = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.BaiJadeTable>());
            baiTable.AddIngredient(ItemID.StoneBlock, 15);
            baiTable.AddIngredient(ItemID.IceBlock, 5);
            baiTable.AddIngredient(yuanS, 3);
            baiTable.AddTile(TileID.WorkBenches);
            baiTable.Register();

            var baiScreen = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.BaiJadeScreen>());
            baiScreen.AddIngredient(ItemID.StoneBlock, 15);
            baiScreen.AddIngredient(ItemID.IceBlock, 5);
            baiScreen.AddIngredient(yuanS, 5);
            baiScreen.AddTile(TileID.WorkBenches);
            baiScreen.Register();

            var xiongAnvil = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.XiongAnvil>());
            xiongAnvil.AddIngredient(ItemID.IronBar, 10);
            xiongAnvil.AddIngredient(ItemID.Bone, 10);
            xiongAnvil.AddIngredient(yuanS, 5);
            xiongAnvil.AddTile(TileID.Anvils);
            xiongAnvil.Register();

            var xiongTotem = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.XiongBoneTotem>());
            xiongTotem.AddIngredient(ItemID.Bone, 20);
            xiongTotem.AddIngredient(yuanS, 5);
            xiongTotem.AddTile(TileID.WorkBenches);
            xiongTotem.Register();

            var tieFurnace = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.TieSmeltingFurnace>());
            tieFurnace.AddIngredient(ItemID.StoneBlock, 20);
            tieFurnace.AddIngredient(ItemID.AshBlock, 10);
            tieFurnace.AddIngredient(yuanS, 5);
            tieFurnace.AddTile(TileID.Anvils);
            tieFurnace.Register();

            var tieRack = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.TieWeaponRack>());
            tieRack.AddIngredient(ItemID.IronBar, 10);
            tieRack.AddIngredient(ItemID.Wood, 10);
            tieRack.AddIngredient(yuanS, 5);
            tieRack.AddTile(TileID.Anvils);
            tieRack.Register();

            var wangOrb = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.WangWaterOrb>());
            wangOrb.AddIngredient(ItemID.Glass, 10);
            wangOrb.AddIngredient(ItemID.CrystalShard, 3);
            wangOrb.AddIngredient(yuanS, 5);
            wangOrb.AddTile(TileID.WorkBenches);
            wangOrb.Register();

            var wangBasin = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.WangWaterBasin>());
            wangBasin.AddIngredient(ItemID.Glass, 10);
            wangBasin.AddIngredient(ItemID.CrystalShard, 3);
            wangBasin.AddIngredient(yuanS, 5);
            wangBasin.AddTile(TileID.WorkBenches);
            wangBasin.Register();

            var zhaoDoor = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ZhaoSecretDoor>());
            zhaoDoor.AddIngredient(ItemID.Obsidian, 10);
            zhaoDoor.AddIngredient(ItemID.Silk, 5);
            zhaoDoor.AddIngredient(yuanS, 5);
            zhaoDoor.AddTile(TileID.WorkBenches);
            zhaoDoor.Register();

            var zhaoLantern = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ZhaoShadowLantern>());
            zhaoLantern.AddIngredient(ItemID.Obsidian, 5);
            zhaoLantern.AddIngredient(ItemID.Torch, 3);
            zhaoLantern.AddIngredient(yuanS, 3);
            zhaoLantern.AddTile(TileID.WorkBenches);
            zhaoLantern.Register();

            var jiaCounter = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.JiaTradingCounter>());
            jiaCounter.AddIngredient(ItemID.Wood, 15);
            jiaCounter.AddIngredient(ItemID.GoldBar, 3);
            jiaCounter.AddIngredient(yuanS, 5);
            jiaCounter.AddTile(TileID.WorkBenches);
            jiaCounter.Register();

            var jiaCoin = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.JiaGoldCoinPile>());
            jiaCoin.AddIngredient(ItemID.GoldCoin, 5);
            jiaCoin.AddIngredient(yuanS, 3);
            jiaCoin.AddTile(TileID.WorkBenches);
            jiaCoin.Register();

            var scatteredFire = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ScatteredCampfire>());
            scatteredFire.AddIngredient(ItemID.Wood, 10);
            scatteredFire.AddIngredient(ItemID.Torch, 3);
            scatteredFire.AddTile(TileID.WorkBenches);
            scatteredFire.Register();

            var scatteredTent = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.FactionBlocks.ScatteredTent>());
            scatteredTent.AddIngredient(ItemID.Wood, 20);
            scatteredTent.AddIngredient(ItemID.Silk, 10);
            scatteredTent.AddTile(TileID.WorkBenches);
            scatteredTent.Register();

            RegisterBreedingRecipes(yuanS);
            RegisterSeedRecipes(yuanS);
        }

        private void RegisterBreedingRecipes(int yuanS)
        {
            var guBox = Recipe.Create(ModContent.ItemType<Content.Items.Breeding.GuBox>());
            guBox.AddIngredient(ItemID.Wood, 10);
            guBox.AddIngredient(ItemID.Silk, 5);
            guBox.AddIngredient(yuanS, 2);
            guBox.AddTile(TileID.WorkBenches);
            guBox.Register();

            var spiritPoolBox = Recipe.Create(ModContent.ItemType<Content.Items.Breeding.SpiritPoolBox>());
            spiritPoolBox.AddIngredient(ItemID.Wood, 15);
            spiritPoolBox.AddIngredient(ItemID.CrystalShard, 5);
            spiritPoolBox.AddIngredient(yuanS, 5);
            spiritPoolBox.AddTile(TileID.WorkBenches);
            spiritPoolBox.Register();

            var battleArena = Recipe.Create(ModContent.ItemType<Content.Items.Breeding.BattleArena>());
            battleArena.AddIngredient(ItemID.IronBar, 10);
            battleArena.AddIngredient(ItemID.Bone, 10);
            battleArena.AddIngredient(yuanS, 8);
            battleArena.AddTile(TileID.Anvils);
            battleArena.Register();

            var guFoodPellet = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.GuFoodPellet>(), 5);
            guFoodPellet.AddIngredient(ItemID.Mushroom, 3);
            guFoodPellet.AddIngredient(ItemID.Gel, 2);
            guFoodPellet.AddTile(TileID.WorkBenches);
            guFoodPellet.Register();

            var spiritMeat = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.SpiritMeat>());
            spiritMeat.AddIngredient(ItemID.Vertebrae, 3);
            spiritMeat.AddIngredient(yuanS, 1);
            spiritMeat.AddTile(TileID.WorkBenches);
            spiritMeat.Register();

            var bloodEssence = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.BloodEssence>());
            bloodEssence.AddIngredient(ItemID.Vertebrae, 5);
            bloodEssence.AddIngredient(ItemID.RottenChunk, 5);
            bloodEssence.AddIngredient(yuanS, 3);
            bloodEssence.AddTile(TileID.DemonAltar);
            bloodEssence.Register();
        }

        private void RegisterSeedRecipes(int yuanS)
        {
            var moonOrchidSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.MoonOrchidSeed>(), 3);
            moonOrchidSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.MoonOrchid>());
            moonOrchidSeed.AddTile(TileID.WorkBenches);
            moonOrchidSeed.Register();

            var riceBagGrassSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.RiceBagGrassSeed>(), 3);
            riceBagGrassSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.RiceBagGrass>());
            riceBagGrassSeed.AddTile(TileID.WorkBenches);
            riceBagGrassSeed.Register();

            var wineGourdFlowerSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.WineGourdFlowerSeed>(), 3);
            wineGourdFlowerSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.WineGourdFlower>());
            wineGourdFlowerSeed.AddTile(TileID.WorkBenches);
            wineGourdFlowerSeed.Register();

            var spiritGrassSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.SpiritGrassSeed>(), 2);
            spiritGrassSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.Environment.SpiritGrass>());
            spiritGrassSeed.AddIngredient(yuanS, 1);
            spiritGrassSeed.AddTile(TileID.WorkBenches);
            spiritGrassSeed.Register();

            var spearBambooSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.SpearBambooSeed>(), 3);
            spearBambooSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.SpearBamboo>());
            spearBambooSeed.AddTile(TileID.WorkBenches);
            spearBambooSeed.Register();

            var healingHerbSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.HealingHerbSeed>(), 3);
            healingHerbSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.HealingHerb>());
            healingHerbSeed.AddTile(TileID.WorkBenches);
            healingHerbSeed.Register();

            var qiHerbSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.QiHerbSeed>(), 2);
            qiHerbSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.QiHerb>());
            qiHerbSeed.AddIngredient(yuanS, 1);
            qiHerbSeed.AddTile(TileID.WorkBenches);
            qiHerbSeed.Register();

            var poisonWeedSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.PoisonWeedSeed>(), 3);
            poisonWeedSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.PoisonWeed>());
            poisonWeedSeed.AddTile(TileID.WorkBenches);
            poisonWeedSeed.Register();

            var boneBanbooSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.BoneBanbooSeed>(), 2);
            boneBanbooSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.BoneBanbooBlock>());
            boneBanbooSeed.AddIngredient(yuanS, 1);
            boneBanbooSeed.AddTile(TileID.WorkBenches);
            boneBanbooSeed.Register();

            var spiritSpringPlantSeed = Recipe.Create(ModContent.ItemType<Content.Items.Seeds.SpiritSpringPlantSeed>(), 1);
            spiritSpringPlantSeed.AddIngredient(ModContent.ItemType<Content.Items.Placeable.GuYueArchitecture.SpiritSpring>());
            spiritSpringPlantSeed.AddIngredient(yuanS, 3);
            spiritSpringPlantSeed.AddTile(TileID.WorkBenches);
            spiritSpringPlantSeed.Register();
        }
    }
}