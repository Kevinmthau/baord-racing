using UnityEngine;

namespace BoardRacing
{
    public static class CarFactory
    {
        public static ArcadeCarController Create(string name, int playerIndex, Color bodyColor, Vector2 position, float rotation)
        {
            var car = new GameObject(name);
            car.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));

            var rigidbody = car.AddComponent<Rigidbody2D>();
            rigidbody.mass = 1.2f;

            var collider = car.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.78f, 1.38f);

            var controller = car.AddComponent<ArcadeCarController>();
            controller.Configure(playerIndex);

            car.AddComponent<RaceProgressTracker>();

            CreateVisual(car.transform, bodyColor);
            controller.ResetCar(position, rotation);

            return controller;
        }

        private static void CreateVisual(Transform parent, Color bodyColor)
        {
            CreateSprite(parent, "Body", bodyColor, Vector2.zero, new Vector2(0.86f, 1.48f), 20);
            CreateSprite(parent, "Cabin", new Color(0.06f, 0.08f, 0.1f), new Vector2(0f, 0.2f), new Vector2(0.5f, 0.48f), 21);
            CreateSprite(parent, "Nose Stripe", Color.white, new Vector2(0f, 0.62f), new Vector2(0.16f, 0.38f), 22);
            CreateSprite(parent, "Rear Bumper", new Color(0.03f, 0.03f, 0.03f), new Vector2(0f, -0.68f), new Vector2(0.76f, 0.12f), 22);
        }

        private static void CreateSprite(Transform parent, string name, Color color, Vector2 localPosition, Vector2 scale, int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = scale;

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = RacingVisuals.WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }
    }
}
