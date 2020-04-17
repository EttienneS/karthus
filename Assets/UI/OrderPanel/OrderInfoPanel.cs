using UnityEngine;
using UnityEngine.UI;

public class OrderInfoPanel : MonoBehaviour
{
    public Text CostLabel;
    public Text DescriptionLabel;
    public Text DetailLabel;
    public Text TitleLabel;

    public string Cost
    {
        get
        {
            return CostLabel.text;
        }
        set
        {
            CostLabel.text = value;
        }
    }

    public string Description
    {
        get
        {
            return DescriptionLabel.text;
        }
        set
        {
            DescriptionLabel.text = value;
        }
    }

    public string Detail
    {
        get
        {
            return DetailLabel.text;
        }
        set
        {
            DetailLabel.text = value;
        }
    }

    public string Title
    {
        get
        {
            return TitleLabel.text;
        }
        set
        {
            TitleLabel.text = value;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(string title, string description, string detail = "", string cost = "")
    {
        Title = title;
        Description = description;
        Detail = detail;
        Cost = cost;
        gameObject.SetActive(true);
    }
}