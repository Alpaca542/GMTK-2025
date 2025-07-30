using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string text;
    public AudioClip audioClip;
    public bool pauseAfter;
}

[System.Serializable]
public class DialogueSequence
{
    public string sequenceName;
    public DialogueLine[] lines;
    public int[] stopAtIndexes;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text dialogueDisplay;
    public GameObject dialogueCanvas;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.02f;
    public float pauseAfterPunctuation = 0.8f;
    public float pauseAfterComma = 0.4f;
    public float pauseAfterSpace = 0.08f;

    [Header("Dialogue Data")]
    public DialogueSequence[] dialogueSequences;
    public DialogueLine[] standaloneDialogues;

    [Header("Audio")]
    public AudioSource typingAudioSource;
    public AudioSource dialogueAudioSource;

    private Coroutine currentTypingCoroutine;
    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool isTyping = false;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        dialogueCanvas.SetActive(false);
    }

    public void StartDialogueSequence(string sequenceName, int startIndex = 0)
    {
        var sequence = Array.Find(dialogueSequences, s => s.sequenceName == sequenceName);
        if (sequence != null)
        {
            StartDialogueSequence(sequence, startIndex);
        }
    }

    public void StartDialogueSequence(DialogueSequence sequence, int startIndex = 0)
    {
        StopCurrentDialogue();
        currentSequence = sequence;
        currentLineIndex = startIndex;
        ShowNextLine();
    }

    public void StartSingleDialogue(int dialogueIndex)
    {
        if (dialogueIndex >= 0 && dialogueIndex < standaloneDialogues.Length)
        {
            StartSingleDialogue(standaloneDialogues[dialogueIndex]);
        }
    }

    public void StartSingleDialogue(DialogueLine dialogue)
    {
        StopCurrentDialogue();
        currentSequence = null;
        currentTypingCoroutine = StartCoroutine(TypeDialogue(dialogue));
    }

    private void ShowNextLine()
    {
        if (currentSequence == null || currentLineIndex >= currentSequence.lines.Length)
        {
            EndDialogue();
            return;
        }

        var currentLine = currentSequence.lines[currentLineIndex];
        currentTypingCoroutine = StartCoroutine(TypeDialogue(currentLine));
    }

    private IEnumerator TypeDialogue(DialogueLine dialogue)
    {
        isTyping = true;
        dialogueCanvas.SetActive(true);
        dialogueDisplay.text = "";

        // Play dialogue audio if available
        if (dialogue.audioClip != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.clip = dialogue.audioClip;
            dialogueAudioSource.Play();
        }

        // Start typing sound
        if (typingAudioSource != null)
        {
            typingAudioSource.loop = true;
            typingAudioSource.Play();
        }

        // Type each character
        foreach (char character in dialogue.text)
        {
            dialogueDisplay.text += character;

            float waitTime = GetWaitTimeForCharacter(character);
            yield return new WaitForSecondsRealtime(waitTime);
        }

        // Stop typing sound
        if (typingAudioSource != null)
        {
            typingAudioSource.loop = false;
            typingAudioSource.Stop();
        }

        isTyping = false;

        // Handle what happens after typing is complete
        if (dialogue.pauseAfter || (currentSequence != null && IsStopIndex(currentLineIndex)))
        {
            yield return new WaitForSecondsRealtime(2f);
            EndDialogue();
        }
        else if (currentSequence != null)
        {
            yield return new WaitForSecondsRealtime(2f);
            ContinueSequence();
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
            EndDialogue();
        }
    }

    private float GetWaitTimeForCharacter(char character)
    {
        switch (character)
        {
            case '.':
            case '!':
            case '?':
                if (typingAudioSource != null) typingAudioSource.loop = false;
                return pauseAfterPunctuation;
            case ',':
                if (typingAudioSource != null) typingAudioSource.loop = false;
                return pauseAfterComma;
            case ' ':
                if (typingAudioSource != null) typingAudioSource.loop = false;
                return pauseAfterSpace;
            default:
                return typingSpeed;
        }
    }

    private bool IsStopIndex(int index)
    {
        return currentSequence != null &&
               currentSequence.stopAtIndexes != null &&
               Array.IndexOf(currentSequence.stopAtIndexes, index) != -1;
    }

    private void ContinueSequence()
    {
        currentLineIndex++;
        ShowNextLine();
    }

    public void SkipCurrentLine()
    {
        if (isTyping && currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);

            // Show full text immediately
            if (currentSequence != null && currentLineIndex < currentSequence.lines.Length)
            {
                dialogueDisplay.text = currentSequence.lines[currentLineIndex].text;
            }

            isTyping = false;
            if (typingAudioSource != null) typingAudioSource.Stop();

            Invoke(nameof(ContinueAfterSkip), 0.5f);
        }
    }

    private void ContinueAfterSkip()
    {
        if (currentSequence != null)
        {
            ContinueSequence();
        }
        else
        {
            EndDialogue();
        }
    }

    public void StopCurrentDialogue()
    {
        if (currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);
            currentTypingCoroutine = null;
        }

        if (typingAudioSource != null) typingAudioSource.Stop();
        if (dialogueAudioSource != null) dialogueAudioSource.Stop();

        isTyping = false;
    }

    public void EndDialogue()
    {
        StopCurrentDialogue();
        dialogueCanvas.SetActive(false);
        currentSequence = null;
        currentLineIndex = 0;

        // Re-enable player controls or other systems
        var playerFollow = Camera.main?.GetComponent<PlayerFollow>();
        if (playerFollow != null)
        {
            playerFollow.enabled = true;
        }

        Time.timeScale = 1f;
    }

    // Public getters for external scripts
    public bool IsDialogueActive => dialogueCanvas.activeSelf;
    public bool IsCurrentlyTyping => isTyping;
}