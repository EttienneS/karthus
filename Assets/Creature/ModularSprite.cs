using UnityEngine;

public class ModularSprite : ICreatureSprite
{
    public SpriteRenderer Face;
    public SpriteRenderer Hair;
    public SpriteRenderer Head;
    public SpriteRenderer Neck;
    public SpriteRenderer Torso;
    public SpriteRenderer LeftArm;
    public SpriteRenderer RightArm;
    public SpriteRenderer LeftSleeve;
    public SpriteRenderer RightSleeve;
    public SpriteRenderer LeftHand;
    public SpriteRenderer RightHand;
    public SpriteRenderer Pelvis;
    public SpriteRenderer LeftLeg;
    public SpriteRenderer RightLeg;
    public SpriteRenderer LeftPant;
    public SpriteRenderer RightPant;
    public SpriteRenderer LeftFoot;
    public SpriteRenderer RightFoot;

    public enum SortOrder
    {
        Botton = 1,
        Mid = 5,
        Top = 10
    }

    public ModularSprite(Creature creature)
    {
        Head = CreateBodypart("Head", creature, 0, 0);
        Hair = CreateBodypart("Hair", creature, 0, 0.35f);
        Face = CreateBodypart("Face", creature, 0, 0, SortOrder.Mid);
        Neck = CreateBodypart("Neck", creature, 0, -1f);

        LeftArm = CreateBodypart("LeftArm", creature, 1, -1.55f);
        RightArm = CreateBodypart("RightArm", creature, -1, -1.55f);
        LeftSleeve = CreateBodypart("LeftSleeve", creature, 1, -1.55f, SortOrder.Mid);
        RightSleeve = CreateBodypart("RightSleeve", creature, -1, -1.55f, SortOrder.Mid);

        LeftHand = CreateBodypart("LeftHand", creature, 1.75f, -2.3f);
        RightHand = CreateBodypart("RightHand", creature, -1.75f, -2.3f);

        Torso = CreateBodypart("Torso", creature, 0, -1.75f, SortOrder.Top);
        Pelvis = CreateBodypart("Pelvis", creature, 0, -2.75f, SortOrder.Top);

        LeftLeg = CreateBodypart("LeftLeg", creature, 0.55f, -3.55f);
        RightLeg = CreateBodypart("RightLeg", creature, -0.55f, -3.55f);
        LeftPant = CreateBodypart("LeftPant", creature, 0.55f, -3.55f, SortOrder.Mid);
        RightPant = CreateBodypart("RightPant", creature, -0.55f, -3.55f, SortOrder.Mid);

        LeftFoot = CreateBodypart("LeftFoot", creature, 0.8f, -4.45f);
        RightFoot = CreateBodypart("RightFoot", creature, -0.8f, -4.45f);

        RightArm.flipX = true;
        RightSleeve.flipX = true;
        RightHand.flipX = true;
        RightLeg.flipX = true;
        RightPant.flipX = true;

        Head.sprite = Game.SpriteStore.HeadSprites.GetRandomItem();
        Face.sprite = Game.SpriteStore.FaceSprites.GetRandomItem();
        Hair.sprite = Game.SpriteStore.HairSprites.GetRandomItem();
        Neck.sprite = Game.SpriteStore.NeckSprites.GetRandomItem();

        LeftArm.sprite = Game.SpriteStore.ArmSprites.GetRandomItem();
        RightArm.sprite = LeftArm.sprite;
        LeftHand.sprite = Game.SpriteStore.HandSprites.GetRandomItem();
        RightHand.sprite = LeftHand.sprite;
        LeftLeg.sprite = Game.SpriteStore.LegSprites.GetRandomItem();
        RightLeg.sprite = LeftLeg.sprite;

        Torso.sprite = Game.SpriteStore.TorsoSprites.GetRandomItem();
        LeftSleeve.sprite = Game.SpriteStore.SleeveSprites.GetRandomItem();
        RightSleeve.sprite = LeftSleeve.sprite;

        Pelvis.sprite = Game.SpriteStore.PelvisSprites.GetRandomItem();
        LeftPant.sprite = Game.SpriteStore.PantSprites.GetRandomItem();
        RightPant.sprite = LeftPant.sprite;

        LeftFoot.sprite = Game.SpriteStore.FootSprites.GetRandomItem();
        RightFoot.sprite = LeftFoot.sprite;

        // tint clothes groups with random colors to match them more evently
        RightSleeve.color = LeftSleeve.color = Torso.color = ColorExtensions.GetRandomColor();
        RightPant.color = LeftPant.color = Pelvis.color = ColorExtensions.GetRandomColor();
        RightFoot.color = LeftFoot.color = ColorExtensions.GetRandomColor();
    }

    private SpriteRenderer CreateBodypart(string name, Creature creature, float x, float y, SortOrder sortorder = SortOrder.Botton)
    {
        var sr = GameObject.Instantiate(creature.BodyPartPrefab, creature.Body.transform);
        sr.name = name;

        sr.transform.localPosition = new Vector2(x, y);
        sr.sortingOrder = (int)sortorder;

        return sr;
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
    }

    internal Sprite IconSprite;

    public Sprite GetIcon()
    {
        if (IconSprite == null)
        {
            var headHeight = (int)Head.sprite.textureRect.width;
            var headWidth = (int)Head.sprite.textureRect.height;

            var iconTex = TextureHelpers.GetSolidTexture(headHeight, headWidth, new Color(0, 0, 0, 0))
                                        .Combine(Head.sprite)
                                        .Combine(Hair.sprite, new Vector2(headWidth * 0.1f, headHeight * 0.40f))
                                        .Combine(Face.sprite, new Vector2(headWidth * 0.25f, headHeight * 0.25f));

            IconSprite = Sprite.Create(iconTex, new Rect(0, 0, headHeight, headWidth), new Vector2());
        }

        return IconSprite;
    }
}