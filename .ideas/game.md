# 🎡 Button Booth — GDD (Game Jam Scope)

> Rogue-lite arcade de fête foraine : appuie sur les bons boutons, gagne des points, transforme ça en argent, achète des bonus via un **gachapon**, et survie à des **prix de parties** qui explosent.

---

## 1) Objectif du jeu
- **Objectif principal :** atteindre des **scores très élevés** en optimisant tes bonus, ton combo et ta prise de risque.
- **Condition de défaite :** si tu **n’as pas assez d’argent** pour payer l’entrée d’une manche → **Game Over**.
- **Philosophie :** “stand forain truqué” → tout devient de plus en plus dur, mais les runs peuvent devenir “broken”.

---

## 2) Lore & ambiance
Tu joues dans une **fête foraine** où un **forain / clown automate** te propose un stand de réflexes :
- “Tu veux rejouer ? Ça coûte plus cher… mais tu peux gagner gros.”
- Le stand est **truqué** : de nouveaux types de boutons apparaissent, des pièges, des règles injustes…
- Ton seul but : **battre le stand** à son propre jeu et empiler des bonus absurdes.

Ambiance : **cute + creepy** (néons, rires mécaniques, jingles d’arcade).

---

## 3) Concept & genre
- **Genre :** arcade reflex + rogue-lite économie
- **Référence :** “Hit-the-light” / boutons lumineux (mais en solo + loop rogue-lite)
- **Point fort :** une économie simple (argent + tickets) + choix stratégiques (jouer / skip / gacha / limites de bonus)

---

## 4) Contrôles & feedback
- Souris / clic : appuyer sur les boutons qui s’allument.
- UI indispensable :
  - Temps restant
  - Score
  - Argent actuel
  - Tickets
  - Manches restantes du tour
  - (plus tard) Compteur d’erreurs “boutons noirs” : `0/3`

---

## 5) Structure : Tours / Manches / Rounds
### Règle clé
- **1 TOUR = 3 MANCHES max**
- Le joueur peut jouer 1, 2 ou 3 manches selon ses moyens/choix.

### Exemple de progression de prix (modifiable)
- Manche 1 : **5€**
- Manche 2 : **12€**
- Manche 3 : **40€**
- Si impossible de payer → **perdu**

> Le jeu est pensé pour tenir la deadline : **boucles courtes, décisions rapides, run claire**.

---

## 6) Départ de run
- Argent : **10€**
- Tickets : **1**
- Limite de bonus : **5** (améliorable via bonus spéciaux)

---

## 7) Gameplay d’une manche (30s)
- Durée : **30 secondes**
- Plateau de boutons avec 3 états :
  - 🟢 **Vert** : bon (récompense max)
  - 🔴 **Rouge** : moyen (récompense faible / “pas optimal”)
  - ⚪ **Neutre** : nul (0)

### Scoring (valeurs de base)
- 🟢 Vert : **+3 points**
- 🔴 Rouge : **+1 point**
- ⚪ Neutre : **+0**

### Fin de manche : conversion du score
- Tu gagnes de l’**argent** en fonction du score de la manche.
- Formule “jam-safe” recommandée (simple à équilibrer) :
  - `Argent gagné = floor(Score / K)` avec K ajustable (ex : 10, 15, 20)
- **+1 ticket** par manche gagnée/jouée (règle simple).

---

## 8) Choix : payer sans jouer (skip)
Le joueur peut :
- **Payer** une manche **sans la jouer**
- Objectif : récupérer **plus de tickets** (stratégie “investissement” / “rush tickets”)

### Ticket bonus de fin de tour (selon manches non jouées)
Principe :
- Plus tu skip de manches, plus tu récupères de tickets en fin de TOUR.

Exemple de barème (à ajuster, valeur “style” proposée) :
- 0 manche restante (tu as tout joué) → **0 ticket bonus**
- 1 manche restante → **+2 tickets bonus**
- 2 manches restantes → **+4 tickets bonus**
- 3 manches restantes → **+6 tickets bonus**

> Objectif design : permettre 2 styles de run :  
> **(A) jouer pour l’argent** vs **(B) skip pour les tickets / bonus**.

---

## 9) Combo (débloqué après le 1er TOUR)
### Déblocage
- Le **combo** n’est pas actif au tout début.
- Il est **débloqué après le 1er TOUR** pour donner une progression claire.

### Fonctionnement (simple & fun)
- Enchaîner des 🟢 **verts** augmente un multiplicateur :
  - Monte lentement, retombe vite si tu rates / cliques un mauvais type.
- Le combo encourage à “lire le plateau” et non spam.

Bonus possibles : ralentir la chute, augmenter la montée, augmenter le cap max.

---

## 10) Boutons noirs (piège late-game)
### Déblocage
- **Tour 3 (Round 9)** : apparition des 🖤 boutons noirs.

### Effet (version jam-safe)
- Appuyer sur un bouton noir :
  - perte d’argent : ex `-5€`
  - compteur d’erreurs noires +1 (reset à chaque TOUR)

- À **3 erreurs noires dans le même TOUR** :
  - tu perds **tout l’argent gagné pendant ce TOUR** (pas ton argent total)
  - le tour continue (mais tu as “ruiné” tes gains)

Objectif : ajouter une pression anti-snowball sans rendre le jeu injuste.

---

## 11) Économie : Argent vs Tickets
### Argent (💰)
- Sert à :
  - payer les manches
  - payer les rerolls
  - survivre à l’escalade des prix

### Tickets (🎟️)
- Sert à :
  - acheter le **gachapon** (objets/bonus)
  - obtenir des bonus plus forts
  - permettre des builds “broken”

> Important : le joueur comprend vite :  
> **Argent = survie**, **Tickets = puissance**.

---

## 12) Gachapon & Shop
### Gachapon (objets/bonus)
- Prix : **1 ticket** par tirage
- Donne un objet aléatoire selon une table de rareté.
- Reroll (si on propose un “choix de 3”) :
  - **2€** au début puis augmente (scaling).

### Reroll scaling (exemple simple)
- 1er reroll : 2€
- 2e : 4€
- 3e : 7€
- 4e : 11€
> Augmentation non-linéaire pour éviter le spam.

---

## 13) Raretés (objets & bonus)
### Raretés proposées
- 🟢 Commun
- 🔵 Rare
- 🟣 Épique
- 🟡 Légendaire
- 🔴 Maudit (optionnel si temps)

### Principe
La rareté **augmente certaines valeurs** d’un même objet (scaling simple).

Exemple de multiplicateurs (proposés) :
- Commun : x1.0
- Rare : x1.3
- Épique : x1.7
- Légendaire : x2.2
- Maudit : x2.5 mais avec un malus

---

## 14) Liste d’objets / bonus (scope Game Jam)
> Objectif : 12–18 objets max, pour rester vite codable.

### A) Bonus “Points”
1. **Peinture Verte**
   - Les 🟢 verts donnent +2 points (base)
   - Rare+ : +3 / +4 / +5

2. **Prime Rouge**
   - Les 🔴 rouges donnent +1 point supplémentaire
   - Rare+ : augmente encore

3. **Multiplicateur Doux**
   - Le combo monte un peu plus vite
   - Rare+ : montée plus rapide / cap plus haut

### B) Bonus “Tickets”
4. **Ticket Drop**
   - Certains boutons (ex: 1 sur 20) donnent 1 ticket au clic (0 point)
   - Rare+ : fréquence augmentée

5. **Machine Généreuse**
   - +1 ticket bonus en fin de TOUR
   - Rare+ : +2 / +3

6. **Skip Reward+**
   - Quand tu payes une manche sans jouer : +1 ticket bonus
   - Rare+ : +2 / +3

### C) Bonus “Temps”
7. **Sablier**
   - +2 secondes par manche
   - Rare+ : +3 / +4 / +5

8. **Ralentisseur**
   - Les boutons restent allumés légèrement plus longtemps
   - Rare+ : plus long

### D) Bonus “Contrôle / QoL”
9. **Anti-Fatigue**
   - Le combo chute moins vite
   - Rare+ : chute beaucoup moins vite

10. **Second Souffle**
   - Une fois par TOUR, si tu rates beaucoup : mini “pause” de 1s (ou freeze boutons)
   - Rare+ : 2 fois / freeze plus long

### E) Bonus “Boutons noirs”
11. **Assurance**
   - Réduit la perte d’argent des boutons noirs
   - Rare+ : réduit davantage

12. **Immunité**
   - La 1ère erreur noire du TOUR est ignorée
   - Rare+ : 2 erreurs ignorées (attention équilibre)

### F) Bonus “Slots”
13. **Poche Supplémentaire**
   - +1 slot bonus (max bonus > 5)
   - Rare+ : +2 (à limiter)

14. **Boîte à Outils**
   - Permet de “jeter” un bonus et récupérer 1 ticket
   - Rare+ : récupère 2 tickets

### G) Objets “Maudits” (si temps)
15. **Deal du Diable**
   - x2 score mais +1 bouton noir supplémentaire apparaît
16. **Caisse Truquée**
   - +X€ immédiat mais les prix augmentent plus vite

> Conseil jam : si vous manquez de temps, gardez A+B+C+D et ajoutez noirs + slots en dernier.

---

## 15) Déblocages / Progression (simple)
- Début : pas de combo
- Après TOUR 1 : combo activé
- TOUR 3 (Round 9) : boutons noirs
- Le reste : uniquement via bonus/gacha (pas de skill tree complexe)

---

## 16) Conditions de victoire / fin
- Pas de “fin” obligatoire : c’est un **score attack**.
- Option : afficher “Tour max atteint” + “Meilleur score” + “Build final”.

---

## 17) Scope strict (pour 26h)
### Must-have (priorité 1)
- Plateau de boutons + activation aléatoire/pattern
- Manche 30s
- Score + conversion en argent
- Payer pour lancer une manche / game over si pas assez
- Tickets + gachapon
- 10–12 bonus

### Nice-to-have (priorité 2)
- Combo après tour 1
- Skip payant sans jouer + bonus tickets fin de tour
- Reroll scaling

### If time (priorité 3)
- Boutons noirs (tour 3 / round 9)
- Bonus “slots” +1
- Objets maudits

---

## 18) Notes d’implémentation (Unity-friendly)
- Boutons = prefabs
- Effets/bonus = ScriptableObjects
- Un seul écran “Arcade Machine”
- UI claire + feedback (sons “ding/buzz”, flash couleur)
- RNG contrôlé (tables de rareté + seed optionnel)

---

## 19) Pitch court (jury)
**Button Booth** est un rogue-lite de fête foraine où chaque partie coûte de plus en plus cher.  
Tu dois faire un score parfait pour gagner assez d’argent, tout en récupérant des tickets pour tirer des bonus au gachapon et créer un build cassé… avant que le stand ne te piège avec de nouveaux boutons dangereux.

---
