using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Vuforia;
using System.Threading.Tasks;

public class QuestManager : MonoBehaviour
{
    int currentQuestIndex;
    int score;

    [SerializeField] Texture2D questionPicture;

    [SerializeField] GameObject questScreen;

    [SerializeField] QuestRoot quests;

    [SerializeField] TextMeshProUGUI questDescriptionText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI endText;
    [SerializeField] GameObject endScreen;

    [SerializeField] RawImage questPicture;

    QuestData currentQuest;

    [SerializeField] Dictionary<QuestData, string> targets;
    [SerializeField] Dictionary<QuestData, Texture> textures;

   [SerializeField] Animation checkMarkAnimation;
    bool isCompleted;

    private void Awake()
    {
        targets = new Dictionary<QuestData, string>();
        textures = new Dictionary<QuestData, Texture>();
        // questTargets = new Dictionary<QuestData, ImageTargetBehaviour>();
    }

    private void Start()
    {
        StartCoroutine(LoadQuests());
    }

    IEnumerator LoadQuests()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://raw.githubusercontent.com/Nimocom/transport-hackathon/main/Data.json"))
        {
            yield return webRequest.SendWebRequest();

            var jsonString = webRequest.downloadHandler.text;

            quests = JsonUtility.FromJson<QuestRoot>(jsonString);

            for (int i = 0; i < quests.Quests.Count; i++)
            {
                yield return TargetsLoader.Instance.RetrieveTextureFromWeb(quests.Quests[i].PictureLink, "Target" + i, (x,y) => { targets.Add(quests.Quests[i], x.TargetName); x.OnTargetStatusChanged += CheckTarget; textures.Add(quests.Quests[i], y); });
            }
        }
    }



    public void ShowQuest(int index)
    {
        isCompleted = false;
        currentQuest = quests.Quests[index];
        questPicture.texture = questionPicture;

        questDescriptionText.SetText(currentQuest.Description);
    }


    public void ShowHint()
    {
        questDescriptionText.SetText(questDescriptionText.text + " " + currentQuest.HintText);
    }

    async public void Skip()
    {
        questPicture.texture = textures[currentQuest];

        await Task.Delay(3000);
        currentQuestIndex++;

        if (currentQuestIndex < targets.Count)
            ShowQuest(currentQuestIndex);
        else
            endScreen.SetActive(true);
    }

    public void SetQuestScreenState()
    {
        questScreen.SetActive(!questScreen.activeSelf);
    }

   async void CheckTarget(ObserverBehaviour observerBehaviour, TargetStatus targetStatus)
    {
        if (questScreen.activeSelf)
            return;

        if (targetStatus.Status == Status.TRACKED)
            if (targets[currentQuest] == observerBehaviour.TargetName)
            {
                checkMarkAnimation.Play();
                questPicture.texture = textures[currentQuest];
                score += 50;
                string endStr = endText.text;
                endStr.Replace("0", score.ToString());
                endText.SetText(endStr);
                scoreText.SetText(score.ToString());
                await Task.Delay(3000);
                questScreen.SetActive(true);
                currentQuestIndex++;
                isCompleted = true;

                if (currentQuestIndex < targets.Count)
                    ShowQuest(currentQuestIndex);
                else
                    endScreen.SetActive(true);

            }
    }

    public void Quit()
    {
        Application.Quit();
    }
}

[Serializable]
public class QuestData
{
    public string Description;
    public string HintText;

    public string PictureLink;
}

[Serializable]
public class QuestRoot
{
    public List<QuestData> Quests;
}