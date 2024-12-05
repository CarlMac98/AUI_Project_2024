import os
import json
import time
from flask import Flask, request, jsonify
from openai import AzureOpenAI
import apiData
import urllib.request

# Flask setup
app = Flask(__name__)

# Azure OpenAI client setup
client = AzureOpenAI(
    azure_endpoint=apiData.azure_endpoint,
    api_key=apiData.api_key,
    api_version="2024-05-01-preview"
)

client_image = AzureOpenAI(
    api_version="2024-02-01",
    azure_endpoint=apiData.azure_image_endpoint,
    api_key=apiData.api_image_key,
)

# Initialize assistant
assistant = client.beta.assistants.create(
    model="AI-assistant",
    instructions =  "You are a helpful AI assistant that helps children in creating stories. "
                    + "Answer in a few lines, the child cannot read too much. You should resolve any conflicts between the children and help them to create a story. "
                    + "Keep answers short and simple. You are speaking directly to the children. "
                    + "Non fargli introdurre nuovi personaggi e non far abbandonare i personaggi presenti nella scena. Non fare la parte dei bambini",
    temperature=0.8,
    top_p=1
)

counter = 0
interactions = 0
question = ""
intervention = ""
n_image = 0

# Load the story configuration
with open('storia.json', 'r') as file:
#with open("C:/Users/Utente/Desktop/Karl/Uni/Poli/AUI/Unity/AUI_Project_2024/storia.json", 'r') as file:
    JSON = json.load(file)

# Create a thread for interactions
thread = client.beta.threads.create()

# Helper function to send messages to Azure OpenAI
def sendMessage(question):
    message = client.beta.threads.messages.create(
        thread_id=thread.id,
        role="user",
        content = question # Replace this with your prompt
    )

    # Run the thread
    run = client.beta.threads.runs.create(
        thread_id=thread.id,
        assistant_id=assistant.id,
        max_prompt_tokens=5000,
        max_completion_tokens=500
    )
    runModel = json.loads(run.model_dump_json(indent=2))

    # Looping until the run completes or fails
    while run.status in ['queued', 'in_progress', 'cancelling']:
        time.sleep(1)
        run = client.beta.threads.runs.retrieve(
            thread_id=thread.id,
            run_id=run.id
        )

    if run.status == 'completed':
        messages = client.beta.threads.messages.list(
            thread_id=thread.id,
            limit=1
        )
        data = json.loads(messages.model_dump_json(indent=2))
        response_text = data['data'][0]['content'][0]['text']['value']
        print(f"Status = {runModel["usage"]}; Narratore:", response_text)
        return response_text

    elif run.status == 'requires_action':
        # the assistant requires calling some functions
        # and submit the tool outputs back to the run
        pass
    else:
        print("status: " + run.status)
        return f"Run Details: {run.model_dump_json(indent=2)}"


intro = "Intervento: " + JSON["inizio"]["introduzione_capitolo"] + f"Il personaggio che li guida sarà {JSON["inizio"]["personaggio_unico"]["nome"]}, {JSON["inizio"]["personaggio_unico"]["descrizione"]}. questa è la introduzione del capitolo. Racconta cosa succede e chiedi ai bambini di continuare la storia, non dare consigli se non te lo chiedono loro, inizia come parlando direttamente a loro"
sendMessage(intro)


# Flask endpoint to handle Unity requests
@app.route('/api/chat', methods=['POST'])
def handle_request_chat():
    global counter, interactions, question, intervention, JSON
    # Parse the incoming JSON
    request_data = request.get_json()
    if not request_data or 'prompt' not in request_data:
        return jsonify({"error": "Invalid request. 'prompt' is required."}), 400

    if counter % 2 == 0:
        question = ""
    
    interactions += 1

    prompt = request_data['prompt']
    print(f"Received prompt: {prompt}")
    question += "\n" + "Utente " + str(counter) + ': ' + prompt

    # Determine the intervention based on story logic

    if interactions == 1:
        intervention = "non fare uscire i personaggi dall'ambiente e non introdurre nuovi personaggi, non far abbandonare il personaggio, non fare la parte dei bambini"
    elif interactions == 4:
        intervention = f"{JSON['inizio']['avventura']['collegamento_scena_successiva']}, non fare la parte dei bambini, racconta cosa succede e chiedigli di decidere, la loro scelta deve essere la stessa."
    elif interactions >= 6:
        intervention = "I bambini devonos scegliere la stessa strada. Solo e soltanto quando ti rendi conto che i bambini hanno scelto, introduci nella tua risposta 'scena successiva', altrimenti non introdurre le parole 'scena successiva'."

    # Send the message to Azure OpenAI and get the response
    if counter % 2 == 1:
        question += f"\nIntervento: {intervention}"
        response = sendMessage(prompt)
    else:
        response = "None"
    print(f"Response: {response}")

    counter = (counter + 1) % 2

    # Return the response back to Unity
    return response

@app.route('/api/image', methods=['POST'])
def handle_request_image():
    global n_image, JSON
    try:
        print(request.get_json()) 
        ramification = request.get_json()
        if ramification and 'imagePrompt' in ramification:
            direction = ramification["imagePrompt"]
        match n_image:
            case 0:
                section = "inizio"
                prompt = JSON[section]["descrizione_scena"]
            case 1:
                section = "fase_intermedia"
                prompt = JSON[section][direction]["descrizione_scena"]
            case 2:
                section = "conclusione"
                prompt = JSON[section]["descrizione_scena"]
            case _:
                prompt = "Una immagine neutra"

        result = client_image.images.generate(
            model="Image-generator", # the name of your DALL-E 3 deployment
            prompt=prompt,
            n=1
        )

        image_url = json.loads(result.model_dump_json())['data'][0]['url']
        urllib.request.urlretrieve(image_url, f"Assets/Images/Backgrounds/image{n_image}.png")
        n_image += 1
        
        return jsonify({"status": "success"}), 200
    except Exception as e:
        print("Error:", e)
        return jsonify({"error": "Server error"}), 400


if __name__ == '__main__':
    # Run the Flask app
    app.run(host='127.0.0.1', port=7000, debug=False)
