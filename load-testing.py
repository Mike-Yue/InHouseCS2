import requests
import uuid
import datetime
import json
import random

def generate_match_data():
    def random_float(min_val, max_val):
        return round(random.uniform(min_val, max_val), 2)

    def random_percentage():
        return round(random.uniform(0, 100), 2)

    def random_team():
        return random.choice(["ct", "t"])

    def generate_player_stat(steam_id, won, tied):
        return {
            "steamId": str(steam_id),
            "steamUsername": steam_username_map[steam_id],
            "kills": random.randint(0, 40),
            "damageAssists": random.randint(0, 10),
            "deaths": random.randint(0, 40),
            "damageDealt": random.randint(0, 3000),
            "mvps": random.randint(0, 5),
            "headshotPercentage": random_percentage(),
            "headshotKillPercentage": random_percentage(),
            "kastRating": random_percentage(),
            "hltvRating": random_float(0, 2),
            "flashAssists": random.randint(0, 10),
            "enemiesFlashed": random.randint(0, 10),
            "highExplosiveGrenadeDamage": random.randint(0, 500),
            "fireGrenadeDamage": random.randint(0, 500),
            "wonMatch": won,
            "tiedMatch": tied,
            "startingTeam": random_team()
        }

    def generate_kill_event(player_ids):
        killer = random.choice(player_ids)
        victim = random.choice([pid for pid in player_ids if pid != killer])
        return {
            "killerSteamId": killer,
            "victimSteamId": victim,
            "headshot": random.choice([True, False]),
            "wallbang": random.choice([True, False]),
            "noScope": random.choice([True, False]),
            "throughSmoke": random.choice([True, False]),
            "midair": random.choice([True, False]),
            "killerFlashed": random.choice([True, False]),
            "victimFlashed": random.choice([True, False]),
            "weaponUsed": random.choice(["ak47", "m4a1", "awp", "galil", "famas", "deagle"])
        }

    # Simulate match scores and outcome
    winning_score = 13
    losing_score = random.randint(0, 12)
    player_count = 10
    half = player_count // 2

    steam_ids = [str(i) for i in range(1, player_count + 1)]
    steam_username_map = {
        '1': "himself",
        '2': "Spare Relations",
        '3': "Lucidit",
        '4': "Wangaroo",
        '5': "ZerO_0",
        '6': "senior",
        '7': "Young",
        '8': "Inoob",
        '9': "Coolguy1002",
        '10': "Dersa",
    }
    random.shuffle(steam_ids)
    winning_ids = steam_ids[:half]
    losing_ids = steam_ids[half:]

    match_stat_per_players = [
        generate_player_stat(steam_id, True, False) for steam_id in winning_ids
    ] + [
        generate_player_stat(steam_id, False, False) for steam_id in losing_ids
    ]

    match_kills = [generate_kill_event(steam_ids) for _ in range(random.randint(5, 20))]

    return {
        "failedToParse": False,
        "matchDataObject": {
            "matchMetadata": {
                "map": random.choice(["mirage", "inferno", "nuke", "train", "ancient", "overpass"]),
                "winningScore": winning_score,
                "losingScore": losing_score
            },
            "matchStatPerPlayers": match_stat_per_players,
            "matchKills": match_kills,
        }
    }

if __name__ == "__main__":
    # Define the URL of the API endpoint
    for i in range(100):
        url = "https://inhousecs2.azurewebsites.net/uploads/url"
        output = requests.post(url, json={
            "demoFingerPrint": uuid.uuid4().hex,
            "matchPlayedAt": datetime.datetime.now(datetime.timezone.utc).isoformat()}
        )
        response_dict = json.loads(output.text)
        id = response_dict["id"]
        match_url = "https://inhousecs2.azurewebsites.net/uploads/{}".format(id)
        output = requests.post(match_url, json=generate_match_data())
        if output.status_code == 200:
            print("Data sent successfully!")
        else:
            print("Failed to send data. Status code:", output.status_code)



