﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    Rigidbody rigidBody;
    AudioSource audioSource;
    FuelSystem fuelSystem;

    enum State { Alive, Dying, Transcending }
    State state = State.Alive;

	// Use this for initialization
	void Start ()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        fuelSystem = GetComponent<FuelSystem>();

	}
	
	// Update is called once per frame
	void Update ()
    {
        if (state == State.Alive && fuelSystem.startFuel > 0)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if(fuelSystem.startFuel <= 0)
        {
            StartDeathSequence();
            return;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) { return; } // ignore collisions when dead

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void StartDeathSequence()
    {

        state = State.Dying;
        audioSource.Stop();
        if (fuelSystem.startFuel <= 0)
            deathParticles.Play();
        else
        {
            audioSource.PlayOneShot(death);
            deathParticles.Play();
        }
        
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(1); // todo allow for more than 2 levels
    }

    private void LoadFirstLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        SceneManager.LoadScene(nextIndex);

    }

    private void RespondToThrustInput()
    {
        if (CrossPlatformInputManager.GetButton("Jump")) // can thrust while rotating
        {
            fuelSystem.fuelConsumptionRate = 8f;
            fuelSystem.ReduceFuel();
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
           // fuelSystem.fuelConsumptionRate = 2f;
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);

        
        if (!audioSource.isPlaying) // so it doesn't layer
        {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();

    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation
       
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (CrossPlatformInputManager.GetButton("Left"))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
            //fuelSystem.fuelConsumptionRate = 0.2f;
            fuelSystem.ReduceFuel();
        }
        else if (CrossPlatformInputManager.GetButton("Right"))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
           // fuelSystem.fuelConsumptionRate = 0.2f;
            fuelSystem.ReduceFuel();
        }
        //transform.Rotate(CrossPlatformInputManager.GetAxis("Horizontal") * Vector3.forward * rotationThisFrame);
        //fuelSystem.ReduceFuel()

        rigidBody.freezeRotation = false; // resume physics control of rotation
    }
}