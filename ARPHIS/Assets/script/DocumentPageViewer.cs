using UnityEngine;

public class DocumentPageViewer : MonoBehaviour
{
    [Header("System Link")]
    public DialogueManager dialogueManager;

    [Header("Page Objects Array")]
    public GameObject[] pages;

    [Header("Navigation Buttons")]
    public GameObject nextButton;    // Drag your 'Next' arrow button here
    public GameObject previewButton; // Drag your 'Preview' (Back) arrow button here

    private int currentPageIndex = 0;

    private void OnEnable()
    {
        // Reset to Page 1 whenever the document opens
        currentPageIndex = 0;
        RefreshVisiblePage();
    }

    public void AdvanceToNextPage()
    {
        currentPageIndex++;

        // If we click NEXT on the last page (Page 5), close the document and resume dialogue
        if (currentPageIndex >= pages.Length)
        {
            if (dialogueManager != null)
            {
                dialogueManager.ResumeDialogueAfterDocument(this.gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
            return;
        }

        RefreshVisiblePage();
    }

    public void RegressToPreviousPage()
    {
        // Prevent going below Page 1
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            RefreshVisiblePage();
        }
    }

    private void RefreshVisiblePage()
    {
        // 1. Loop through all pages to only enable the active one
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        // 2. Handle PREVIEW (Back) Button Visibility
        // Hide on Page 1 (index 0), show on all other pages
        if (previewButton != null)
        {
            previewButton.SetActive(currentPageIndex > 0);
        }

        // 3. Handle NEXT Button Visibility
        // Hide completely on the last page (index 4 out of 5 pages)
        if (nextButton != null)
        {
            nextButton.SetActive(currentPageIndex < pages.Length - 1);
        }
    }
}