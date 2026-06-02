using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonArchitect.Systems
{
    public class RiddlePopupUI : MonoBehaviour
    {
        private Canvas canvas;
        private System.Action<bool> onResolved;
        private GameObject panelGO;

        private static readonly string[][] riddles =
        {
            new[] { "Tengo ciudades, pero no casas.\nTengo montanas, pero no arboles.\nTengo agua, pero no peces.\n\n¿Que soy?", "Un mapa", "Un rio", "Un desierto" },
            new[] { "Cuanto mas quitas, mas grande soy.\n\n¿Que soy?", "Un agujero", "Una sombra", "El hambre" },
            new[] { "No tengo pies, pero siempre corro.\nNo tengo boca, pero siempre murmuro.\n\n¿Que soy?", "Un rio", "El viento", "El tiempo" },
            new[] { "Tengo manos pero no puedo aplaudir.\n\n¿Que soy?", "Un reloj", "Un arbol", "Una estatua" },
            new[] { "Me puedes romper sin tocarme.\n\n¿Que soy?", "Una promesa", "El silencio", "Un sueno" },
            new[] { "Siempre vengo pero nunca llego.\n\n¿Que soy?", "El manana", "La muerte", "El invierno" },
            new[] { "Cuanto mas seco, mas mojo.\n\n¿Que soy?", "Una toalla", "El sol", "La arena" },
            new[] { "Todos me pisan pero yo no piso a nadie.\n\n¿Que soy?", "El suelo", "Una alfombra", "Una sombra" },
            new[] { "Tiene corona pero no es rey.\n\n¿Que es?", "Un diente", "Un gallo", "Una montana" },
            new[] { "Vuela sin alas, llora sin ojos.\n\n¿Que es?", "Una nube", "El viento", "El humo" },
            new[] { "Soy ligero como una pluma, pero ni el hombre mas fuerte puede sostenerme mas de un minuto.\n\n¿Que soy?", "El aliento", "Un pensamiento", "El humo" },
            new[] { "Tiene cuello pero no cabeza.\n\n¿Que es?", "Una botella", "Una jirafa", "Una camisa" },
            new[] { "Nace grande y muere pequeno.\n\n¿Que es?", "Un lapiz", "Una vela", "El dia" },
            new[] { "Tiene dientes pero no muerde.\n\n¿Que es?", "Un peine", "Una sierra", "Un tenedor" },
            new[] { "Cuanto mas hay, menos se ve.\n\n¿Que es?", "La oscuridad", "La niebla", "El humo" },
            new[] { "Tiene hojas pero no es arbol.\nTiene lomo pero no es animal.\n\n¿Que es?", "Un libro", "Una mesa", "Un cuchillo" },
            new[] { "Se llena cada noche y se vacia cada dia.\n\n¿Que es?", "La cama", "El cielo", "La luna" },
            new[] { "Mientras mas le quitas, mas grande es.\n\n¿Que es?", "Una fosa", "Una deuda", "El olvido" },
            new[] { "Puedo viajar por el mundo sin salir de mi rincon.\n\n¿Que soy?", "Un sello", "Un libro", "Un mapa" },
            new[] { "Tiene patas pero no camina.\n\n¿Que es?", "Una mesa", "Un cangrejo muerto", "Una silla" },
            new[] { "Sube y baja pero no se mueve.\n\n¿Que es?", "La temperatura", "Una escalera", "El ascensor" },
            new[] { "No es ni humano ni animal, pero tiene cara.\n\n¿Que es?", "Una moneda", "La luna", "Un reloj" },
            new[] { "Corre sin pies, hiere sin manos.\n\n¿Que es?", "El rio", "El viento", "El tiempo" },
            new[] { "Que se moja mientras seca?\n\n¿Que es?", "La toalla", "El sol", "El aire" },
            new[] { "Oro no es, plata no es.\nAbre la cortina y veras lo que es.\n\n¿Que es?", "El platano", "El sol", "Un huevo" },
            new[] { "Tengo agujas pero no se coser.\n\n¿Que soy?", "Un reloj", "Un pino", "Un erizo" },
            new[] { "Que tiene un ojo pero no puede ver?\n\n¿Que es?", "Una aguja", "Una tormenta", "Una cerradura" },
            new[] { "Que puede llenar una habitacion sin ocupar espacio?\n\n¿Que es?", "La luz", "El aire", "El sonido" },
            new[] { "Tiene llaves pero no abre puertas.\n\n¿Que es?", "Un piano", "Un teclado", "Un cerrajero" },
            new[] { "Que se rompe en el agua pero no en la tierra?\n\n¿Que es?", "Una ola", "El hielo", "Un reflejo" },
            new[] { "Sin mi no hay fuego.\nSin mi no hay oscuridad.\n\n¿Que soy?", "El oxigeno", "La noche", "Una cerilla" },
            new[] { "Tengo brazos pero no manos.\nTengo esfera pero no soy planeta.\n\n¿Que soy?", "Un reloj", "Un arbol", "Una lampara" },
            new[] { "Que puedes oir pero no ver ni tocar?\n\n¿Que es?", "La voz", "El eco", "El trueno" },
            new[] { "Camina sobre cuatro patas por la manana, dos al mediodia y tres por la noche.\n\n¿Que es?", "El ser humano", "Un perro viejo", "El tiempo" },
            new[] { "Que tiene raices que nadie ve,\nes mas alta que los arboles,\ny sin embargo nunca crece?\n\n¿Que es?", "Una montana", "Una torre", "La sombra" },
            new[] { "Si me nombras, desaparezco.\n\n¿Que soy?", "El silencio", "Un secreto", "La oscuridad" },
            new[] { "Que se puede coger pero no tocar?\n\n¿Que es?", "Un resfriado", "El viento", "Una idea" },
            new[] { "Que es tan fragil que al decir su nombre se rompe?\n\n¿Que es?", "El silencio", "Una burbuja", "La confianza" },
            new[] { "Paso por el agua y no me mojo.\nPaso por el fuego y no me quemo.\n\n¿Que soy?", "La sombra", "El humo", "El viento" },
            new[] { "Que tiene cabeza, tiene cola, pero no tiene cuerpo?\n\n¿Que es?", "Una moneda", "Un cometa", "Un alfiler" },
        };

        public void Initialize(Vector3 worldPos, Camera cam, System.Action<bool> callback, int forceIndex = -1)
        {
            onResolved = callback;

            int riddleIndex = forceIndex >= 0 && forceIndex < riddles.Length
                ? forceIndex
                : Random.Range(0, riddles.Length);
            var riddle = riddles[riddleIndex];

            int[] shuffled = { 0, 1, 2 };
            for (int i = 2; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            int correctButton = System.Array.IndexOf(shuffled, 0);

            CreateCanvas();
            CreateContent(riddle[0], new[] { riddle[shuffled[0] + 1], riddle[shuffled[1] + 1], riddle[shuffled[2] + 1] }, correctButton);
        }

        private void CreateCanvas()
        {
            var canvasGO = new GameObject("RiddleCanvas");
            canvasGO.transform.SetParent(transform);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 95;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bgGO.AddComponent<CanvasRenderer>();
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.85f);
            bgImg.raycastTarget = true;
        }

        private void CreateContent(string question, string[] answers, int correctBtn)
        {
            panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvas.transform, false);
            var panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0.15f);
            panelRT.anchorMax = new Vector2(0.8f, 0.85f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            panelGO.AddComponent<CanvasRenderer>();
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.12f, 0.95f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.85f);
            titleRT.anchorMax = new Vector2(0.95f, 0.95f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
            titleGO.AddComponent<CanvasRenderer>();
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "La Estatua Parlante susurra...";
            titleTMP.fontSize = 30;
            titleTMP.color = new Color(0.9f, 0.75f, 0.3f);
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.fontStyle = FontStyles.Bold;

            var questionGO = new GameObject("Question");
            questionGO.transform.SetParent(panelGO.transform, false);
            var questionRT = questionGO.AddComponent<RectTransform>();
            questionRT.anchorMin = new Vector2(0.1f, 0.45f);
            questionRT.anchorMax = new Vector2(0.9f, 0.82f);
            questionRT.offsetMin = questionRT.offsetMax = Vector2.zero;
            questionGO.AddComponent<CanvasRenderer>();
            var questionTMP = questionGO.AddComponent<TextMeshProUGUI>();
            questionTMP.text = question;
            questionTMP.fontSize = 26;
            questionTMP.color = Color.white;
            questionTMP.alignment = TextAlignmentOptions.Center;
            questionTMP.enableWordWrapping = true;

            for (int i = 0; i < answers.Length; i++)
            {
                float yMin = 0.08f + (2 - i) * 0.12f;
                float yMax = yMin + 0.10f;

                var btnGO = new GameObject($"Answer_{i}");
                btnGO.transform.SetParent(panelGO.transform, false);
                var btnRT = btnGO.AddComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0.15f, yMin);
                btnRT.anchorMax = new Vector2(0.85f, yMax);
                btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
                btnGO.AddComponent<CanvasRenderer>();

                var btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.15f, 0.12f, 0.2f, 1f);

                var btn = btnGO.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.3f, 0.25f, 0.4f, 1f);
                colors.pressedColor = new Color(0.4f, 0.3f, 0.5f, 1f);
                btn.colors = colors;

                int capturedIndex = i;
                int capturedCorrect = correctBtn;
                btn.onClick.AddListener(() => OnAnswerClicked(capturedIndex == capturedCorrect));

                var textGO = new GameObject("Text");
                textGO.transform.SetParent(btnGO.transform, false);
                var textRT = textGO.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(10, 0);
                textRT.offsetMax = new Vector2(-10, 0);
                textGO.AddComponent<CanvasRenderer>();
                var textTMP = textGO.AddComponent<TextMeshProUGUI>();
                textTMP.text = answers[i];
                textTMP.fontSize = 22;
                textTMP.color = Color.white;
                textTMP.alignment = TextAlignmentOptions.Center;
            }
        }

        private void OnAnswerClicked(bool correct)
        {
            Destroy(panelGO);
            ShowResultPanel(correct);
        }

        private void ShowResultPanel(bool correct)
        {
            var resultGO = new GameObject("ResultPanel");
            resultGO.transform.SetParent(canvas.transform, false);
            var resultRT = resultGO.AddComponent<RectTransform>();
            resultRT.anchorMin = new Vector2(0.25f, 0.25f);
            resultRT.anchorMax = new Vector2(0.75f, 0.75f);
            resultRT.offsetMin = resultRT.offsetMax = Vector2.zero;
            resultGO.AddComponent<CanvasRenderer>();
            var resultImg = resultGO.AddComponent<Image>();
            resultImg.color = correct
                ? new Color(0.05f, 0.15f, 0.05f, 0.95f)
                : new Color(0.2f, 0.05f, 0.05f, 0.95f);

            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(resultGO.transform, false);
            var headerRT = headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.05f, 0.7f);
            headerRT.anchorMax = new Vector2(0.95f, 0.92f);
            headerRT.offsetMin = headerRT.offsetMax = Vector2.zero;
            headerGO.AddComponent<CanvasRenderer>();
            var headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
            headerTMP.text = correct ? "Correcto!" : "Incorrecto...";
            headerTMP.fontSize = 38;
            headerTMP.fontStyle = FontStyles.Bold;
            headerTMP.color = correct ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
            headerTMP.alignment = TextAlignmentOptions.Center;

            var flavorGO = new GameObject("Flavor");
            flavorGO.transform.SetParent(resultGO.transform, false);
            var flavorRT = flavorGO.AddComponent<RectTransform>();
            flavorRT.anchorMin = new Vector2(0.1f, 0.48f);
            flavorRT.anchorMax = new Vector2(0.9f, 0.68f);
            flavorRT.offsetMin = flavorRT.offsetMax = Vector2.zero;
            flavorGO.AddComponent<CanvasRenderer>();
            var flavorTMP = flavorGO.AddComponent<TextMeshProUGUI>();
            flavorTMP.fontSize = 24;
            flavorTMP.color = new Color(0.85f, 0.85f, 0.85f);
            flavorTMP.alignment = TextAlignmentOptions.Center;
            flavorTMP.enableWordWrapping = true;
            flavorTMP.text = correct
                ? "La estatua sonrie y te ofrece una recompensa."
                : "La estatua frunce el ceno y te castiga.";

            var rewardGO = new GameObject("Reward");
            rewardGO.transform.SetParent(resultGO.transform, false);
            var rewardRT = rewardGO.AddComponent<RectTransform>();
            rewardRT.anchorMin = new Vector2(0.1f, 0.3f);
            rewardRT.anchorMax = new Vector2(0.9f, 0.48f);
            rewardRT.offsetMin = rewardRT.offsetMax = Vector2.zero;
            rewardGO.AddComponent<CanvasRenderer>();
            var rewardTMP = rewardGO.AddComponent<TextMeshProUGUI>();
            rewardTMP.fontSize = 26;
            rewardTMP.alignment = TextAlignmentOptions.Center;
            rewardTMP.richText = true;
            rewardTMP.text = correct
                ? "<color=#FFD700>+5 Oro</color>  <color=#66FF66>+10 XP</color>"
                : "<color=#FF6666>-3 HP</color>";

            var btnGO = new GameObject("ContinueBtn");
            btnGO.transform.SetParent(resultGO.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.25f, 0.08f);
            btnRT.anchorMax = new Vector2(0.75f, 0.24f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.AddComponent<CanvasRenderer>();

            var btnImg2 = btnGO.AddComponent<Image>();
            btnImg2.color = new Color(0.2f, 0.2f, 0.3f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg2;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.5f, 1f);
            colors.pressedColor = new Color(0.4f, 0.4f, 0.55f, 1f);
            btn.colors = colors;

            bool capturedCorrect = correct;
            btn.onClick.AddListener(() => onResolved?.Invoke(capturedCorrect));

            var btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnTextRT = btnTextGO.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = btnTextRT.offsetMax = Vector2.zero;
            btnTextGO.AddComponent<CanvasRenderer>();
            var btnTextTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnTextTMP.text = "Continuar";
            btnTextTMP.fontSize = 24;
            btnTextTMP.color = Color.white;
            btnTextTMP.alignment = TextAlignmentOptions.Center;
        }
    }
}
