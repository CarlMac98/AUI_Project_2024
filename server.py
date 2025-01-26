import os
import json
import time
from flask import Flask, request, jsonify
from openai import AzureOpenAI
import apiData
import urllib.request
import signal
import sys


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
    instructions =  "Sei un'AI assistente che aiuta i bambini a creare storie. Rispondi in modo breve e semplice, perché i bambini non possono leggere troppo. Risolvi eventuali conflitti tra i bambini e aiutali a sviluppare la storia. Parla direttamente con loro. Non permettere di introdurre nuovi personaggi o di abbandonare quelli già presenti nella scena. Non impersonare i bambini.",
    temperature=0.8,
    top_p=1
)
#assistant = client.beta.assistants.retrieve(assistant_id="asst_G8vGIkFJcg6za5raTm0xLcaS")

counter = 0
interactions = 0
question = ""
intervention = ""
n_image = 0
section = "inizio"
percorso = "percorso_1"
JSON = {}


# Create a thread for interactions
thread = client.beta.threads.create()

def cleanup():
    global counter, interactions, question, intervention, section, percorso, n_image, assistant
    # Call your cleanup function here
    try:
        outcome = client.beta.threads.delete(thread.id)
        outcome2 = client.beta.assistants.delete(assistant.id)
        counter = 0
        interactions = 0
        question = ""
        intervention = ""
        n_image = 0
        section = "inizio"
        percorso = "percorso_1"
        
        #print(f"Cleanup response: {outcome}")
    except Exception as e:
        print(f"Error during cleanup: {e}")

def create_assistant():
    global assistant
    assistant = client.beta.assistants.create(
        model="AI-assistant",
        instructions =  "Sei un'AI assistente che aiuta i bambini a creare storie." + 
                        " Rispondi in modo breve e semplice," +
                        " perché i bambini non possono leggere troppo." +
                        " Risolvi eventuali conflitti tra i bambini e aiutali a" + 
                        " sviluppare la storia. Parla direttamente con loro." +
                        " Non permettere di introdurre nuovi personaggi o di" + 
                        " abbandonare quelli già presenti nella scena. Non impersonare i bambini.",
        temperature=0.8,
        top_p=1
    )

def int_intro(interactions):
    nonInt = "Se credi non ci sia bisogno di intervenire scrivi 'non intervengo' altrimenti intervieni e "
    if interactions == 1:
        return nonInt + "non fare uscire i personaggi dall'ambiente e non introdurre nuovi personaggi, non far abbandonare il personaggio, non fare la parte dei bambini"
    elif interactions == 8:
        return f"Intervieni, {JSON["inizio"]["collegamento_scena_successiva"]["fine_capitolo"]}, non fare la parte dei bambini, racconta cosa succede e chiedigli di decidere tra {JSON["inizio"]["collegamento_scena_successiva"]["percorso_1"]} e {JSON["inizio"]["collegamento_scena_successiva"]["percorso_2"]}, la loro scelta deve essere la stessa."
    elif interactions >= 10 and interactions < 16:
        return nonInt + f"I bambini devono scegliere la stessa strada. Solo e soltanto quando ti rendi conto che i bambini hanno scelto, introduci nella tua risposta 'scena successiva' e la scelta fatta tra {JSON["inizio"]["collegamento_scena_successiva"]["percorso_1"]} e {JSON["inizio"]["collegamento_scena_successiva"]["percorso_2"]} (metti le parole esatte, non aggiungere parole in mezzo), altrimenti non introdurre le parole 'scena successiva'."
    elif interactions >= 18:
        return f"Intervieni e scegli tu la strada da seguire tra {JSON["inizio"]["collegamento_scena_successiva"]["percorso_1"]} e {JSON["inizio"]["collegamento_scena_successiva"]["percorso_2"]} e metti le parole 'scena successiva' e il percorso scelto nella tua risposta."

def int_intermedia(interactions):
    nonInt = "Se credi non ci sia bisogno di intervenire scrivi 'non intervengo' altrimenti intervieni e "
    if interactions == 1:
        return nonInt + "non fare uscire i personaggi dall'ambiente e non introdurre nuovi personaggi, non far abbandonare il personaggio, non fare la parte dei bambini"
    elif interactions >= 10:
        return f"Intervieni, {JSON["fase_intermedia"]["collegamento_scena_successiva"]["fine_capitolo"]}, non fare la parte dei bambini, racconta cosa succede e introduci nella tua risposta 'scena successiva'."


def int_concl(interactions):
    nonInt = "Se credi non ci sia bisogno di intervenire scrivi 'non intervengo' altrimenti intervieni e "
    if interactions == 1:
        return nonInt + "non fare uscire i personaggi dall'ambiente e non introdurre nuovi personaggi, non far abbandonare il personaggio, non fare la parte dei bambini"
    elif interactions >= 10:
        return f"Intervieni, {JSON["conclusione"]["conclusione"]}, non fare la parte dei bambini, racconta cosa succede come conclusione e introduci nella tua risposta 'scena successiva'."

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

def determina_percorso(response):
    if JSON["inizio"]["collegamento_scena_successiva"]["percorso_1"].lower() in response.lower():
        return "percorso_1"
    else:
        return "percorso_2"

# Flask endpoint to handle Unity requests
@app.route('/api/create_story', methods=['POST'])
def handle_story_creation():
    global JSON
    if not request.get_json() or 'n_story' not in request.get_json():
        n_story = 0
    else:
        n_story = request.get_json()["n_story"]
    # Load the story configuration

    try:
        with open(f'storia_{n_story}.json', 'r') as file:
        #with open("C:/Users/Utente/Desktop/Karl/Uni/Poli/AUI/Unity/AUI_Project_2024/storia.json", 'r') as file:
            JSON = json.load(file)
        intro = JSON["inizio"]["introduzione_capitolo"] + f" Il personaggio che guida i protagonisti sarà {JSON["inizio"]["personaggio_unico"]["nome"]}, {JSON["inizio"]["personaggio_unico"]["descrizione"].casefold()} I protagonisti si trovano in {JSON["inizio"]["descrizione_scena"].casefold()}"
    except Exception as e:
        print(f"Error loading JSON: {e}")
        return jsonify({"error": f"Server error: {e}"}), 400
    
    return intro, 200

@app.route('/api/help', methods=['POST'])
def handle_aiuto():
    request_data = request.get_json()
    if not request_data or 'utente' not in request_data:
        return jsonify({"error": "Invalid request. 'prompt' is required."}), 400

    utente = request_data['utente']
    domanda = request_data['domanda']
    prompt = f"Intervento: {utente} ti sta parlando in privato solo per questo prompt, devi aiutarlo per qualsiasi dubbio lui abbia, non far andare avanti la storia, questa la sua domanda.\n {utente}:'{domanda}'"
    response = sendMessage(prompt)
    return response, 200

@app.route('/api/summary', methods=['POST'])
def handle_summary():
    prompt = "Intervento: fai un riassunto un po' dettagliato della storia, se ci sono più capitoli dividili, non far andare avanti la storia, racconta cosa è successo finora. Il contenuto deve essere adatto per bambini. Metti subito il riassunto, non mettere 'Va bene' 'Certo!' o cose del genere, metti subito il testo utile."
    response = sendMessage(prompt)
    return response, 200

@app.route('/api/reset', methods=['POST'])
def handle_reset():
    global counter, interactions, question, intervention, section, percorso, n_image, thread, assistant

    cleanup()
    assistant = client.beta.assistants.create(
        model="AI-assistant",
        instructions =  "Sei un'AI assistente che aiuta i bambini a creare storie. Rispondi in modo breve e semplice, perché i bambini non possono leggere troppo. Risolvi eventuali conflitti tra i bambini e aiutali a sviluppare la storia. Parla direttamente con loro. Non permettere di introdurre nuovi personaggi o di abbandonare quelli già presenti nella scena. Non impersonare i bambini.",
        temperature=0.8,
        top_p=1
    )
    thread = client.beta.threads.create()
    return jsonify({"status": "success"}), 200

@app.route('/api/init', methods=['POST'])
def handle_initial_call():
    global section, percorso
    match section:
        case "inizio":
            intro = "Intervento: " + JSON["inizio"]["introduzione_capitolo"] + f"Il personaggio che li guida sarà {JSON["inizio"]["personaggio_unico"]["nome"]}, {JSON["inizio"]["personaggio_unico"]["descrizione"]}. Questa è la introduzione del capitolo. Saluta i bambini e riassumi molto brevemente ciò che accade e chiedi a loro di continuare la storia, non dare consigli se non te lo chiedono loro. Il contenuto deve essere adatto per bambini."
        case "fase_intermedia":
            intro = "Intervento: " + JSON["fase_intermedia"][percorso]["introduzione_capitolo"] + f"Il personaggio che li guida sarà {JSON["fase_intermedia"][percorso]["personaggio_unico"]["nome"]}, {JSON["fase_intermedia"][percorso]["personaggio_unico"]["descrizione"]}. Questa è la introduzione del capitolo. Riassumi molto brevemente ciò che accade e chiedi a loro di continuare la storia, non dare consigli se non te lo chiedono loro. Il contenuto deve essere adatto per bambini."
        case "conclusione":
            intro = "Intervento: " + JSON["conclusione"]["introduzione_capitolo"] + f"Il personaggio che li guida sarà {JSON["conclusione"]["personaggio_unico"]["nome"]}, {JSON["conclusione"]["personaggio_unico"]["descrizione"]}. Questa è la introduzione del capitolo. Riassumi molto brevemente ciò che accade e chiedi a loro di continuare la storia, non dare consigli se non te lo chiedono loro. Il contenuto deve essere adatto per bambini."
    response = sendMessage(intro)
    return response, 200

@app.route('/api/chat', methods=['POST'])
def handle_request_chat():
    global counter, interactions, question, intervention, JSON, section, percorso
    # Parse the incoming JSON
    request_data = request.get_json()
    if not request_data or 'prompt' not in request_data:
        return jsonify({"error": "Invalid request. 'prompt' is required."}), 400

    if counter % 2 == 0:
        question = ""
    
    interactions += 1

    prompt = request_data['prompt']
    print(f"Received prompt: {prompt}")
    #question += "\n" + "Utente " + str(counter) + ': ' + prompt
    question += "\n" + prompt

    # Determine the intervention based on story logic
    match section:
        case "inizio":  
            intervention = int_intro(interactions)
        case "fase_intermedia":
            intervention = int_intermedia(interactions)
        case "conclusione":        
            intervention = int_concl(interactions)

    # Send the message to Azure OpenAI and get the response
    if counter % 2 == 1:
        question += f"\nIntervento: stai parlando a entrambi i bambini, {intervention}"
        #print(f"Question: {question}")
        response = sendMessage(question)
    else:
        response = "None"
    #print(f"Response: {response}")

    counter = (counter + 1) % 2
    next_scene = False
    conclusione = False
    scala = 0
    if "scena successiva" in response.lower():
        next_scene = True
        if section == "inizio":
            section = "fase_intermedia"
            percorso = determina_percorso(response)
            if percorso == "percorso_1":
                scala = 1
            elif percorso == "percorso_2":
                scala = 2
        elif section == "fase_intermedia":
            section = "conclusione"
            scala = 3
        elif section == "conclusione":
            conclusione = True
        else:
            section = "inizio"
        interactions = 0
    #if "content_filter" in response.lower() or "token" in response.lower():
    #    response = "Non ho capito, potete ripetere quello che volete raccontare? " + response
    if "non intervengo" in response.lower():
        response = "Non intervengo"

    if 'incomplete_details' in request_data:
        response = "Non ho capito, potete ripetere quello che volete raccontare?"
    # Return the response back to Unity
    return jsonify({"response": response, "next_scene": next_scene, "scala": scala, "conclusion": conclusione }), 200

@app.route('/api/image', methods=['POST'])
def handle_request_image():
    global n_image, JSON, section, percorso
    try:
        #print(request.get_json()) 
        #ramification = request.get_json()
        #if ramification and 'imagePrompt' in ramification:
        #    direction = ramification["imagePrompt"]
        match section:
            case "inizio":
                prompt = JSON["inizio"]["descrizione_scena"]
                n_image = 0
            case "fase_intermedia":
                prompt = JSON["fase_intermedia"][percorso]["descrizione_scena"]
                n_image = 1
            case "conclusione":
                prompt = JSON["conclusione"]["descrizione_scena"]
                n_image = 2
            case _:
                prompt = "Una immagine neutra"
                n_image = 3

        result = client_image.images.generate(
            model="Image-generator",
            prompt=prompt,
            n=1
        )

        image_url = json.loads(result.model_dump_json())['data'][0]['url']
        directory = f"Assets/Images/Backgrounds/image{n_image}.png"
        #urllib.request.urlretrieve(image_url, directory)
        
        return jsonify({"url": image_url, "dir": directory}), 200
    except Exception as e:
        print("Error:", e)
        return jsonify({"error": "Server error"}), 400

@app.route('/shutdown', methods=['POST'])
def shutdown():
    cleanup()
    func = request.environ.get('werkzeug.server.shutdown')
    if func is None:
        raise RuntimeError('Not running with the Werkzeug Server')
    func()
    return 'Server shutting down...', 200

if __name__ == '__main__':
    # Run the Flask app
    app.run(host='127.0.0.1', port=7001, debug=False)
