using UnityEngine;
using System.Collections;

public class DollyZoom : MonoBehaviour
{
    private Camera cameraComponent;

    public float dollyFOV;
    float normalFOV;

    float width;

    // Use this for initialization
    void Start()
    {
        cameraComponent = this.GetComponent<Camera>();
        normalFOV = cameraComponent.fieldOfView;
        width = -transform.position.z * 2 * Mathf.Tan(Mathf.Deg2Rad * cameraComponent.fieldOfView / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            StartCoroutine(DollyZoomIn());
        }
    }

    public IEnumerator DollyZoomIn()
    {
        while (this.cameraComponent.fieldOfView < dollyFOV)
        {
            cameraComponent.fieldOfView += 0.1f;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -width / (2 * Mathf.Tan(Mathf.Deg2Rad * cameraComponent.fieldOfView / 2)));
            yield return null;
        }
        yield return null;
    }

    public IEnumerator DollyZoomOut()
    {
        while (this.cameraComponent.fieldOfView > normalFOV)
        {
            cameraComponent.fieldOfView -= 0.1f;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -width / (2 * Mathf.Tan(Mathf.Deg2Rad * cameraComponent.fieldOfView / 2)));
            yield return null;
        }
        yield return null;
    }
}
