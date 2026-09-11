"""Build the Snowhill NPC roster from the recovered spawn data + wiki cross-reference.

Inputs : npc_spawns_named.json (this dir), ../strings/en_us.json, ../wiki/*, client/custom/FabledRealmsAreas.xml
Outputs: snowhill_roster.csv, snowhill_roster_table.md (this dir)
Run from the repo root: python research/npc-spawns/build_snowhill_roster.py
"""
import json, re, math, collections, csv, os, urllib.parse

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
HERE = os.path.dirname(os.path.abspath(__file__))
WIKI = os.path.join(ROOT, 'research', 'wiki')

d = json.load(open(os.path.join(HERE, 'npc_spawns_named.json'), encoding='utf-8'))
xml = open(os.path.join(ROOT, 'client', 'custom', 'FabledRealmsAreas.xml'), encoding='utf-8', errors='ignore').read()
vols = [dict(re.findall(r'(\w+)="([^"]*)"', a)) for a in re.findall(r'<AreaDefinition ([^>]*)>', xml)]
tv = [v for v in vols if re.search('snowhill|hotspring', v['name'], re.I)]


def inside(v, s):
    x, y, z = s['x'], s['y'], s['z']
    if v.get('shape') == 'sphere':
        return math.dist((x, y, z), (float(v['x1']), float(v['y1']), float(v['z1']))) <= float(v['radius'])
    try:
        xs = sorted([float(v['x1']), float(v['x2'])]); zs = sorted([float(v['z1']), float(v['z2'])])
        return xs[0] <= x <= xs[1] and zs[0] <= z <= zs[1]
    except KeyError:
        return False


def kind(s):
    mf = (s['model_file'] or '').lower(); nm = (s['name'] or '')
    if mf.startswith('hsg_'):
        return 'housing'
    if 'harvestable' in mf or mf.startswith('sg_collection'):
        return 'harvest'
    if mf.startswith(('sg_', 'sh_', 'evnt_', 'mkt_', 'loot_', 'powerup_', 'farming_', 'ss_deco')) or mf in ('invisible_cube.adr', 'mail_01.adr', 'ball_m_snow.adr'):
        return 'prop'
    if 3600 <= s['model_id'] <= 3699 or 'spawner' in mf or 'frostfang' in nm.lower():
        return 'creature'
    return 'npc'


hit = [s for s in d if any(inside(v, s) for v in tv)]
# Edge NPCs the wiki places "south of Snowhill" but which sit outside its ambience volumes.
for s in d:
    if s['name'] in ('Ree Peatpants', 'Gerold') and s not in hit:
        hit.append(s)
kinds = collections.Counter(kind(s) for s in hit)
print('Snowhill volumes: spawns', len(hit), dict(kinds))
npcs = [s for s in hit if kind(s) == 'npc']

GENERIC = {'Human', 'Pixie', 'Human Child', 'Penguin', 'Dog', 'Deer', 'Squirrel', 'Blue Squirrel', 'Cow', 'Goat', 'Billygoat',
           'Rabbit', 'Chicken', 'Raven', 'Wolf', 'Dwarf', 'Miner', 'Chugawug', 'Robgoblin', 'robgoblin', 'crab', 'fish', 'Customer',
           'kid1', 'kid2', 'Builder1', 'Builder2', 'volleyballer', 'tourist', 'lifeguard', 'Noob Warrior', 'Tower Defender',
           'Snowhill Neighbor', 'Pleading Son', 'Smart Kid', 'Storyteller', "Iggy's Owner", 'Curious Dog', 'Backyard Dog',
           'Annoyed Penguin', 'Wild Eagle', 'Wild Hawk', 'Flying Saucer', 'Toppled Snowman', 'Snowball Pile', 'Party Lights',
           'Gift of Health', 'Gift of Power', 'Coin Drop', 'PCA Trickster'}
MOUNTS_ITEMS = {'Hydra Ride', 'The Fire Hawk', 'Striped T-Rex', 'Scaled Dragon', 'Air T-Rex', 'Futuristic Bicycle', 'Jar of Jitter Bugs',
                'Gloamstone Hoverboard', 'Chrome Drake', 'S.M.A.C.M.', 'Stone Saber-Tooth Tiger', 'Toy Maker Outfit', 'Miss Santa Outfit',
                'Toy Soldier Outfit', 'Santa Claus Outfit', 'Iggy', 'Zinx', 'Zoey', 'Permit'}

wiki_pages = {f[:-9] for f in os.listdir(os.path.join(WIKI, 'npcs'))}
cat_npcs = set(open(os.path.join(WIKI, 'category_npcs.txt'), encoding='utf-8').read().splitlines())
qg = set(open(os.path.join(WIKI, 'category_quest_givers.txt'), encoding='utf-8').read().splitlines())
merch = set(open(os.path.join(WIKI, 'category_merchants.txt'), encoding='utf-8').read().splitlines())
snowcat = set(open(os.path.join(WIKI, 'categories', 'Snowhill.txt'), encoding='utf-8').read().split('\n')[1].strip().split(' | '))

DIALOGUE = {
    'Flynn': 'wiki quote + Noisy Neighbors / Noise Permit transcripts',
    'Clara Chatterhag': 'Noisy Neighbors transcript',
    'Momma Meepster': 'Misplaced Meeps transcript',
    'Foreman Hetfield': 'wiki quote',
    'Fritti Bluebelle': 'wiki quote',
    'Roland Sporeling': 'wiki quote',
    'Cragara': 'wiki quotes (shop open / closed)',
    'Bruce': 'wiki quotes + Thugawug Sneak! battle lines',
    'Umari': 'wiki boss lines',
    'Abominable Snowman': 'wiki boss lines',
    'Tiger the Party Animal': 'A Luau / Party for Pets / Winter Party transcripts',
}
ROLE = {n: 'Merchant' for n in ['Tommy', 'Dempsy', 'Spratt', 'Reba', 'Sonja', 'Skye', 'Fladnag', 'Chip Numbwing', 'Cragara',
                                'Hank Fisticuffs', 'Vaal', 'Wayland', 'Foreman Hetfield', 'Assistant Chef Edward']}
ROLE.update({
    'Tommy': 'Merchant (Archer gear, Aug 2013)', 'Dempsy': 'Merchant (Brawler gear, by Post Office)', 'Spratt': 'Merchant (Medic gear)',
    'Reba': 'Merchant (Postman gear, by Post Office)', 'Sonja': 'Merchant (Warrior gear)', 'Skye': 'Merchant (Chef gear, by cooking table)',
    'Fladnag': 'Merchant (Wizard gear, by Post Office)', 'Chip Numbwing': 'Merchant (Fisherman)', 'Cragara': 'Mysterious Merchant (roaming)',
    'Assistant Chef Edward': 'Merchant (Chef supplies)',
    'Mayor Crystalline': 'Mayor; Brawler / Medic / Card Duelist quest hub (front of Town Hall)',
    'Frostpetal': 'Quest giver: Meet the Mayor!, Ninja: Return to the Mentor (right of town entrance)',
    'Tevin': 'Brawler trainer: Way of the Brawler',
    'Drill Sergeant Dewey': 'Warrior job unlock (front of Town Hall)',
    'Steele': 'Warrior trainer (Town Hall, blocks until lvl 5)',
    'Morninglory': 'Warrior trainer',
    'Loryn': 'Postman trainer (4 quests)',
    'Garrison Gold': 'Card Duelist job unlock',
    "Lucca De'Flor": 'Chef trainer (5 quests; cooking table at 2 Icecrest Ct)',
    'Ree Peatpants': 'Ninja contract giver (4 repeatable contracts; south of town)',
    'Gerold': 'Brawler quest giver: Growler Encroachment / The Growler Report (south of town)',
    'Flynn': 'Snowhill Stage caretaker; Noisy Neighbors -> Noise Permit',
    'Clara Chatterhag': 'Starts Noisy Neighbors',
    'Momma Meepster': '8-Bit Month event: Misplaced Meeps (near Big J\'s)',
    'Candi Ivy': 'Snow Days event quest giver (10 quests, by the Gifting Tree)',
    'Bert': 'Penguin postman', 'Ernie': 'Penguin postman',
    'Big J': "Owner of Big J's cafe", 'Mr. Twinkle': "Idle NPC outside Big J's", 'Senari': 'Idle NPC',
    'Arci Joan': 'NPC beside Easy Penguin Defense minigame', 'Kiel': 'Idle NPC',
    'Roland Sporeling': 'Diamondback Raceway host (above Frostfang Caverns)',
    'Tiger the Party Animal': 'Event party host / merchant (on the stage)',
    'Jonathon Forkpath': 'Quest giver (Defenders Medal; wiki spells "Johnathon")',
    'Vaelen Warpwatcher': 'Warpstone druid (warp NPC)', 'Coin Farmer Lubag': 'Farming NPC', 'Yoink': 'Robgoblin banker',
    'Spindle': 'Dwarf NPC', 'Autumn Mistflower': 'NPC (also Npcs.json id 1198)', 'Scarlet Shadeveil': 'Idle NPC',
    'Holly Singsong': 'Idle NPC', 'Milus Featherfeet': 'Elder pixie NPC', 'Captain Ironsides': 'Pirate NPC (TCG character)',
    'Everett': 'Miner NPC', 'Tad Slopeslider': 'Idle NPC', 'Tanda T. Toes': 'Idle NPC', 'Linnie': 'Idle NPC', 'Downey': 'Idle NPC',
    'Annabelle': 'Idle NPC', 'Calvin Coldcastle': 'Idle NPC', 'Freddy MacIsaac': 'Idle NPC', 'Willie': 'Idle NPC', 'Nikki': 'Idle NPC',
    'Cinn': 'Farmer NPC', 'Grog': 'Chugawug NPC', 'Robgoblin Builder': 'Snow Days event', 'Crafty Robgoblin': 'Snow Days event',
    'Chip Numbwing': 'Merchant (Fisherman)',
})
WIKINAME = {'Jonathon Forkpath': 'Johnathon Forkpath', 'Assistant Chef Edward': 'Assistant Chief Edward'}

rows = collections.OrderedDict()
for s in npcs:
    n = s['name']
    if n in GENERIC or n in MOUNTS_ITEMS:
        continue
    k = (n, s['model_file'])
    if k not in rows:
        rows[k] = {'name': n, 'name_id': s['name_id'], 'model_id': s['model_id'], 'model_file': s['model_file'],
                   'x': round(s['x'], 1), 'y': round(s['y'], 1), 'z': round(s['z'], 1), 'heading': s['heading_deg'],
                   'area': s['area'], 'n': 0}
    rows[k]['n'] += 1

out = []
for r in rows.values():
    w = WIKINAME.get(r['name'], r['name']); pg = re.sub(r'[^A-Za-z0-9]', '_', w)
    r['wiki'] = 'yes' if (pg in wiki_pages or w in cat_npcs or w in snowcat) else 'no'
    r['quest_giver'] = 'yes' if w in qg else ''
    r['merchant'] = 'yes' if w in merch else ''
    r['dialogue'] = DIALOGUE.get(w, '')
    r['role'] = ROLE.get(r['name'], '')
    r['url'] = 'https://freerealms.fandom.com/wiki/' + urllib.parse.quote(w) if r['wiki'] == 'yes' else ''
    out.append(r)
out.sort(key=lambda r: (r['wiki'] != 'yes', r['quest_giver'] != 'yes', r['name']))

with open(os.path.join(HERE, 'snowhill_roster.csv'), 'w', newline='', encoding='utf-8') as f:
    w = csv.DictWriter(f, fieldnames=list(out[0].keys())); w.writeheader(); w.writerows(out)
print('roster rows', len(out), '| with wiki page', sum(r['wiki'] == 'yes' for r in out),
      '| quest givers', sum(r['quest_giver'] == 'yes' for r in out), '| with dialogue', sum(bool(r['dialogue']) for r in out))

md = ['| NPC | Role (wiki / job pages) | Position (x, y, z) / heading | Wiki page | Dialogue on record | NameId / model |',
      '|---|---|---|---|---|---|']
for r in out:
    link = f"[yes]({r['url']})" if r['wiki'] == 'yes' else 'no'
    md.append(f"| {r['name']} | {r['role']} | ({r['x']}, {r['y']}, {r['z']}) / {r['heading']} | {link} | {r['dialogue'] or '-'} | {r['name_id']} / `{r['model_file']}` |")
open(os.path.join(HERE, 'snowhill_roster_table.md'), 'w', encoding='utf-8').write('\n'.join(md) + '\n')

MISSING = ['Sorin', 'Buren', 'Cronyn', 'Therin', 'Trixi', 'Brody Sparfist', 'Smitty', 'Flynn', 'Snowi', 'Silversnow', 'Hank Fisticuffs',
           'Foreman Hetfield', 'Valinda', 'Carrie', 'Morgulg', 'Vaal', 'Wayland', 'Yaren Sunstare', 'Jeni Shortfuse', 'Michi', 'Sampson',
           'Fastvi Frostflutter', 'Umari', 'Madam Zelda', 'Bruce', 'Baron von Darkcheat (character)', 'Abominable Snowman', 'Pear Jam']
S = {v: k for k, v in json.load(open(os.path.join(ROOT, 'research', 'strings', 'en_us.json'), encoding='utf-8')).items()}
for m in MISSING:
    print('  no-pos:', m, '| wiki:', ('yes' if re.sub(r'[^A-Za-z0-9]', '_', m) in wiki_pages else 'no'),
          '| string id:', S.get(m.replace(' (character)', '')))
