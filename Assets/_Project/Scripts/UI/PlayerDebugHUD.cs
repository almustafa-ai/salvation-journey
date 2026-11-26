using UnityEngine;
using SalvationJourney.Player;

namespace SalvationJourney.UI
{
    public class PlayerDebugHUD : MonoBehaviour
    {
        [Header("Player References")]
        [SerializeField] private PlayerStateController playerState;
        [SerializeField] private PlayerMovementController playerMovement;

        [Header("Display Settings")]
        [SerializeField] private bool showDebugHUD = true;

        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle panelStyle;

        private void OnGUI()
        {
            if (!showDebugHUD)
                return;

            if (playerState == null || playerMovement == null)
                return;

            InitializeStyles();

            GUILayout.BeginArea(
                new Rect(15, 15, 270, 285),
                panelStyle
            );

            GUILayout.Label(
                "ELIAS | PLAYER DEBUG",
                titleStyle
            );

            GUILayout.Space(8);

            DrawResource(
                "Health",
                playerState.Health,
                new Color(0.85f, 0.25f, 0.25f)
            );

            DrawResource(
                "Resolve",
                playerState.Resolve,
                new Color(0.25f, 0.55f, 0.9f)
            );

            DrawResource(
                "Stamina",
                playerState.Stamina,
                new Color(0.3f, 0.75f, 0.35f)
            );

            GUILayout.Space(10);

            DrawStatus("Grounded", playerMovement.IsGrounded);
            DrawStatus("Moving", playerMovement.IsMoving);
            DrawStatus("Sprinting", playerMovement.IsSprinting);
            DrawStatus("Exhausted", playerMovement.IsExhausted);
            DrawStatus("Stillness", playerState.IsUsingStillness);

            GUILayout.EndArea();
        }

        private void DrawResource(
            string resourceName,
            float value,
            Color barColor)
        {
            GUILayout.Label(
                $"{resourceName}: {value:F1} / 100",
                labelStyle
            );

            Rect barRect = GUILayoutUtility.GetRect(
                240f,
                9f
            );

            Color originalColor = GUI.color;

            GUI.color = new Color(
                0.2f, 0.2f, 0.2f, 1f
            );

            GUI.DrawTexture(
                barRect,
                Texture2D.whiteTexture
            );

            float fillAmount = Mathf.Clamp01(
                value / 100f
            );

            GUI.color = barColor;

            GUI.DrawTexture(
                new Rect(
                    barRect.x,
                    barRect.y,
                    barRect.width * fillAmount,
                    barRect.height
                ),
                Texture2D.whiteTexture
            );

            GUI.color = originalColor;

            GUILayout.Space(5);
        }

        private void DrawStatus(
            string statusName,
            bool active)
        {
            GUILayout.Label(
                $"{statusName}: {(active ? "YES" : "NO")}",
                labelStyle
            );
        }

        private void InitializeStyles()
        {
            if (titleStyle != null)
                return;

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor =
                new Color(0.95f, 0.8f, 0.45f);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 13;
            labelStyle.normal.textColor = Color.white;

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.padding =
                new RectOffset(12, 12, 10, 10);
        }
    }
}