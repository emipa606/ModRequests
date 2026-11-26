
(English and French versions will follow)

(Les versions anglaises et françaises suivront)


警告：本MOD还处于Beta阶段，我自己虽然排除了许多BUG，但由于mod逻辑太过复杂，无法保证没有别的bug。因此使用前请记得备份你的存档！


# 外交联姻增强版


	本MOD又名“外交婚姻和永久联盟”(Diplomacy Marriage & Permanent Alliance)，引入了类似Paradox的《十字军之王》系列的一个重要元素--联姻和姻亲同盟。
	
	你可以让你的殖民地领袖的儿女和盟友的要人结婚来达成永久同盟，让你的盟友进一步成为你的姻亲家人，为你提供更多帮助而不用担心关系降低。
	
	即使结婚后他/她不再属于你的殖民地，但依然会在盟友阵营继续为你工作，维护两家的外交关系，也会和配偶经常返回你的殖民地。


# 主要内容：

	- 在你的文化有了领袖后，如果他/她有某位成年未婚子女，关系超过75的盟友阵营会随机提出结亲。
	- 联姻达成后，该子女会离开你加入盟友阵营。此后根据你的小人的社交技能，和盟友的关系每天都会上涨（mod选项里可调具体算法）。只要联姻一直有效，关系能轻松地几乎时刻保持最大值100。自然衰退也很快就会被抵消。
	- 由于每天都会涨大量关系值，你可以尽情享受永久盟友为你提供的各种好处，比如呼叫商队，援军，而无需担心关系度下降过快。
	- 你嫁娶过去的小人会在永久盟友中传播你的文化，甚至可能转化整个阵营。
	- 永久同盟达成后，会有一些后续随机事件，比如两个小人所在的NPC商队会遭到第三方敌对阵营的攻击，躲到你的基地寻求庇护。你需要不惜代价保护二人来保卫永久同盟关系（提醒：保护你无法控制的NPC小人并不是那么容易的）。
	- 殖民地领袖的儿子/女儿偶尔会得思乡病，拉着配偶回来住一段时间，成为受你控制的临时殖民者，这时候你可以抓紧时间给他们训练技能，配备武器装备，植入各种设备，让他们以后回归盟友阵营后战斗中更不容易死。甚至有生物技术DLC的可以让他们永远不老不死，好让永久同盟无限持续下去。


# MOD兼容性

	该MOD没有修改任何原版内容，只是单纯加了我自己写的几个新随机事件，因此正常情况下不会造成任何冲突。
	但有些MOD和本MOD在一起用时，有些情况下可能会产生一些意外的效果。比如:
	- VE文化拓展提供的一些新模因中，有些模因是会根据小人的技能或皇家头衔等级强行修改领袖的，而领袖更替可能会导致永久同盟失效。
	- Dynamic Diplomacy偶尔会有小概率随机改变某个派系的文化，可能让你的小人转化姻亲盟友的努力前功尽弃（选项里可以关闭）。
	使用类似MOD时需要时刻注意，不要让它们影响永久同盟的判定条件（见FAQ）。不过对于任何修改NPC阵营和外交关系的MOD，这类问题始终都存在。


# 需要DLC
	文化



# FAQ:

	- More Faction Interactions（MFI）这个MOD不是已经有类似事件了吗？为什么还要专门做一个MOD？
	MFI是我一直在玩的MOD，我个人非常喜欢，我的灵感也确实来自MFI（这也是我为什么给本MOD取名外交婚姻“增强版”的缘故），甚至包括许多代码都参考了作者的写法。
	但MFI的事件太多，不可能在单独一个事件上做的太深入。那个外交联姻事件仅仅是让你牺牲一个随机的小人，换取很少的一次性关系上涨，其实在我看来并没有什么大用。
	本MOD参考P社的《十字军之王》中通过联姻来结盟的机制，极大拓展了该事件，让外交婚姻有了和《十字军之王》类似的用途，几乎可以把一家NPC阵营永久绑在你的战车上为你服务。
	和MFI不同的是，联姻会给你长期的关系加成，而不是一次性的，更重要的是，嫁娶到盟友阵营的小人并不是如MFI那样一去不复返，即使到了姻亲盟友后，依然会经常和你互动，让你真正觉得他/她依然是属于你的小人。

	- 我如果同时用MFI，是否会让两个类似的外交婚姻事件冲突？
	不会，我虽然借鉴了MFI的不少源码，但本MOD完全是独立的，和MFI之间没有任何依赖关系。两种外交婚姻事件都有机会触发。

	- 什么情况下永久同盟会失效？
	1. 联姻一方或双方小人死亡
	2. 盟友派系被灭（原版除了玩家没有人能消灭NPC派系，但Dynamic Diplomacy或Rim War这类MOD却有可能）
	3. 玩家文化领袖换人。要保证永久同盟生效，玩家的文化领袖必须和联姻的小人有亲生父母关系，换成任何无关人员当领袖都会让永久同盟立刻失效。但如果领袖只是从父亲换成母亲，或母亲换成父亲则不受影响。
	4. 好感度过低。虽然每天自动涨好感，但如果一次性降到0以下依然可能导致永久同盟破裂。所以姻亲盟友在场时需要特别注意友伤，特别是如果你喜欢用火焰魔王和蜈蚣的话。

	- 永久同盟失效后会发生什么？
	永久同盟失效后，根据失效的原因，有些情况下你嫁娶过去的小人会只身返回，也有些则会死亡，或永远留在对方派系。另外，如果永久同盟失效时好感度够高，该阵营依然会继续做你的常规盟友。

	- 永久同盟失效后，我还能再和盟友建一个新的永久同盟吗？
	可以，只要满足联姻事件触发条件，组成永久同盟的次数无上限，和同一个或不同的NPC阵营都可以。但同一时刻只能有一个永久同盟。
	
	- 我和永久同盟的关系怎么依然会下降？
	永久同盟并不会消除自然好感衰退，以及呼叫援军等操作的好感消耗，只是每天会增加好感来抵消上述好感度下降值，
	
	- 是否兼容已有的存档？
	可以。但和大部分MOD一样，中途卸载的后果我无法保证。


# 支持语言：
	目前我自己做了三种语言的版本（也是我自己现实中通晓的语言）
	- 简体中文
	- 英语
	- 法语
	对于懂得如何添加XML语言文件的，也欢迎翻译更多语言，但目前因为MOD尚未完成，以后可能还需要修改。


# 目前已知问题：
	- 如果只有文化DLC，有时候机械族敌人无法生成。但在有生物技术DLC时我却没遇到过此类问题。
	- 如果用超凡枢纽开了二周目，尚未测试永久同盟是否会继续。因为我对超凡枢纽任务的机制并不清楚。


# 鸣谢

	这是我第一个环世界MOD，特别感谢以下MOD的作者：

	- More Faction Interactions: 那个外交婚姻事件虽然简单，却是我做这个MOD将其进一步拓展的灵感来源，同时公开的源码也给了我很大帮助。
	- Factional War: 感谢作者公开源码以及清晰的注释，让我在短时间内学会了如何生成NPC队伍并给他们下指令互相开撕，因为我的MOD同样用到了这类功能。
	- Dynamic Diplomacy: 同样感谢作者公开源码，我也从里面学到不少事件代码写法。

	以上三个MOD也都是我平时一直在玩的，在我学习了它们的源码后更喜欢了，强烈推荐。

	同时也感谢Discord上mod-development频道中热心回答我一些技术问题的一些大佬（抱歉因为我提问次数不多，没记住你们的ID）。


# 其它推荐的相关MOD:

	- Not My Fault: 该MOD可以让盟友不会因为一些不合理的理由而降低好感，特别是某些并非你的过错的情况（如盟友商队在你的殖民地正好撞上了机械集群），从而减少永久同盟破裂的可能。
	- Better Traders: 让NPC商人有更多的存货和银子可以和你交易。这样更容易依赖你的姻亲盟友给你不间断供货。
	

# 后续计划：

	该MOD还远远没有完成，目前的版本只是Beta阶段的最初版，我还有不少灵感尚未实现。
	比如：
	- 如果两位关键小人死在战斗中，给一段宽限期让玩家可以用复活液来避免永久同盟破裂。
	- 进一步引入十字军之王的继承人战争，打宣称之类的元素，比如帮你的姻亲儿媳/女婿在盟友派系内部夺权，成为领袖，进一步巩固关系，得到更多好处。
	- 添加更多和永久盟友的互动，甚至包括帮助盟友攻打敌人，统治全星球（类似Empire那样的MOD，但区别是你不需要管理这么多殖民地，只需要通过姻亲盟友来实现）
	- 更多联姻夫妇二人遇险求救事件，增加维持永久同盟的难度。（比如某些拯救两位小人的任务，如果不完成小人就会死亡等等）
	- 提供某种途径让玩家主动选择某个NPC阵营提议联姻（如无线电，或远行队访问NPC基地等）
	- 如果有生物技术DLC，是否还能让联姻的两位小人怀孕生子？不在地图上的NPC小人是否技术上可行我还不太确定。
	- 能否设定让姻亲盟友的商队给你降价？（我没找到相关的实现方法）

	上面有些想法我技术上可能不知道怎么实现，欢迎提供宝贵建议，特别是代码示例。如果有愿意帮我一起实现的，也欢迎直接在Github上上传你们的Pull Request。

	Github
	https://github.com/Ionfrigate12345/Rimworld_DiplomaticMarriagePlus
	
	
=====================================English====================================


WARNING: This mod is still in Beta. I already fixed alot of bugs in my real playthough, but due to the logical complexity, it may still not be bug-free. So backup your save before using it!


# Diplomatic Marriage Plus (DMP)


This mod is also known as "Diplomacy Marriage & Permanent Alliance". It s heavily inspired by the famous Paradox game - Crusader Kings.
	
You can now form a permanent alliance by marrying a child of your colony leader to an important person of one of your ally, making the latter your in-laws family, providing you more help without worrying about goodwill.
	
Even if your child no longer belongs to your colony after marriage, they will continue to work on your behalf inside your ally by maintain diplomatic relation, also they will occasionally return to your colony for a while.


# Features:

- After your ideology has a leader, if he/she has an adult unmarried child, allied faction with a goodwill of more than 75 will randomly propose marriage.
- After the marriage is concluded, the child will leave you and join the ally. From then on, according to their social skill, the goodwill with your ally will increase every day (the algorithm can be adjusted in the mod config). As long as the marriage remains active, the goodwill can easily stay as the maximum value of 100 almost all the time. Natural decay can be quickly offset.
- You can enjoy the various benefits provided by your permanent ally, such as calling caravans and reinforcements, without worrying about the goodwill declining too quickly.
- Your colonist will spread your ideology inside your ally, even ending up converting the entire faction.
- After the permanent alliance, there will be some subsequent random events. The ally caravan with the couple among them may randomly come to seek refuge in your colony. In order to keep the alliance, you need to protect the couple at all cost in the incoming raid against the caravan. (FYI: sometimes it wont be easy to protect a pawn not under your control!)
- Your child will occasionally get homesick and come back to you colony as temporary pawn with their spouse. Think of it as an opportunity to train them skills, give them weapons, armors or implants, so that they will less likely to die in the future battles after returning to the ally. With Biotech DLC you can even make them immortal so that the permanent alliance can last forever.


# COMPATIBILITY

This mod does not change any vanilla content, but simply adds a few new random events managed by my own script, so it s highly unlikely to conflict anything.
However, if you use some mods together, some unexpected effects may occur in some cases. For example:
- Some of the new memes provided by Vanilla Ideology Expanded will forcibly change your ideology leader based on their skills or royalty titles, which may unexpectedly end the permanent alliance.
- Dynamic Diplomacy will occasionally change the ideology of an AI faction, which may also void your effort of converting your permanent ally (you can turn it off in the options).
You need to always be careful when using mods like these, especially try not let them void the conditions of your permanent alliance (see FAQ). But for a mod altering AI faction, this kind of problems always exist.


# Hard requirement of DLC
	Ideology



# FAQ:

- Isn’t there already a similar feature in More Faction Interactions (MFI) ? Why do you need to make another dedicated mod?
MFI is a mod that I have been playing for long. I personally love it very much. This mod is also inspired by MFI, which is why I name it Diplomatic Marriage PLUS. Even many of my codes are learned from MFI.
However, there are too many various events in MFI, and it is impossible to go too deep on a single one. The diplomatic marriage event  in MFI only allows you to sacrifice a random colonist in exchange for a small one-time goodwill boost. Actually it doesn't really worth IMO.
This mod however is inspired by Paradox game Crusader Kings, which greatly expands the event of diplomatic marriage, giving it a real purpose of forming a permanent union with an NPC faction, making them at your service in long term. In a word, it will give you long-term benefit rather than a small one-time bonus. 
More importantly, your child leaving you for marriage wont be lost forever like the similar event in MFI. They will still interact with you frequently, making you truly feel that they still belong to you.

- If I use MFI at the same time, will two similar diplomatic marriage events conflict?
No, I just inspired by MFI, but this mod is 100% standalone. If you have MFI, both diplomatic marriage events may trigger independently.

- Under what circumstances will a permanent alliance expire?
1. Death of one or both colonists involved in the marriage
2. The allied faction is destroyed (Impossible in vanilla but it may happen with mods such as Dynamic Diplomacy or Rim War)
3. Changing your ideology leader. To maintain the alliance, your ideology leader must have parenthood relation with the married pawn. Replacing any unrelated person as new leader will immediately invalidate the permanent alliance. But if the leadership is simply passed from father to mother, or vice versa, it will not be affected.
4. The goodwill is too low. Even it increases quickly each day, but falling below 0 may still lead to the breakdown of the permanent alliance. So you need to pay special attention to friendly fire when your ally is present, especially if you are a fan of mechs like Diabolus or Centipede.

- What happens after the permanent alliance expires?
After the permanent alliance expires, depending on the reason, the child of your leader will return alone, die, or stay in the ex permanent ally forever. In addition, if the goodwill is high enough when the permanent alliance expires, they will still stay as your vanilla ally.

- After the permanent alliance expires, can I form a new permanent alliance?
Yes, as long as the triggering conditions are met, there is no limit of the total number of permanent alliances you can have per game, either with the same faction or a different one. But only one permanent alliance is allowed at a time.
	
- Why is my goodwill with the permanent alliance still declining?
The permanent alliance will not eliminate the natural goodwill decay, nor the goodwill cost for calling reinforcements or caravans etc.. It will only increase the goodwill each day to negate them.

- Save compatible?
Yes. But like most mods, I can't garantee the safety of removing it from existing saves. 


# Supported languages:
At present, I have made versions in three languages (which are also the languages I speak IRL).
	- Chinese Simplified
	- English
	- French
It is absolutely welcomed to translate into more languages, provided you know how to do it in these xml files. but since this mod is still incomplete, the text may still be changed in the future.


# Currently known issues:
- With only Ideology DLC, sometimes mech enemies cannot be generated. But I didn't encounter such issue with Biotech and Royalty DLC active.
- I dont know what will happen if you start round 2 with Archonexus, for I have no idea how that quest works.


# Acknowledgments

This is my first mod of Rimworld. Many thanks to the authors of the following mods:

- More Faction Interactions: Although the diplomatic marriage event is simple, it s where my inspiration comes from to make this mod. At the same time, the source code on github also helped me a lot.
- Factional War: Thanks to the author's open source and clear comments, by which I learned how to generate NPC armies and order them to fight each other, for this mod also involves the same feature.
- Dynamic Diplomacy: Thanks also to the author for publishing the source code. I also learned a lot about incident management.

These mods are what I have been playing all the time. Now I like them even more after learning from their source code. Highly recommanded.

I would also like to thank those who helped me in the Discord mod-development channel, they answered some of my technical questions with incredible speed. (Sorry I dont remember your ID for I didn’t ask many questions).


# Other recommended MODs:

- Not My Fault: This mod prevents NPC factions from lowering goodwill for some unreasonable causes, especially those which are not your fault (such as an ally's caravan clashing a mechanical cluster on your colony map). It helps reducing the likelihood of unfairly breacking up your permanent alliance.
- Better Traders: Let NPC traders have more goods and silver to trade, making your permanent ally easier to supply you.
	

# Follow-up plan:

This mod is far from being complete. The current version is only Beta. I still have a lot of inspirations that have not yet been implemented.
For example:
- If the couple dies in battle, allow a grace period to resurrect them before breaking permanent alliance.
- Furthermore introduce Crusader King elements like Successor War, supressing claims, such as helping your son/daughter in-laws become the leader of your ally and consolidate further the permanent alliance for more benefits.
- Add more interactions with permanent ally, even including helping allies attack enemies and rule the entire planet (similar to the mod Empire, but instead you dont need to manage so many colonies, but let your ally do it for you)
- More incidents where two married couples are in danger and seek help, making it  harder to maintain the permanent alliance. (For example, some quests to save two colonists, and making them die if the quest fails, etc.)
- Provide a way to actively choose a faction for marriage (such as radiostation, or player caravan visiting the NPC settlement, etc.)
- With Biotech DLC, can the couple become pregnant and have kids? I'm not sure if it's technically feasible for world pawns from other factions.
- Maybe let you have a rebate from traders of your permanent ally? (I didn't find a way to implement this)

I may not know how to implement some of the above ideas technically, so any suggestion or especially code examples are welcome. 
If anyone wants to help me improve this mod, you can also make your pull request directly on my github.

Github
https://github.com/Ionfrigate12345/Rimworld_DiplomaticMarriagePlus



	
=====================================Français====================================

ATTENTION : Ce mod est encore en bêta. J'ai déjà corrigé pleins de bugs dans ma partie réelle, mais en raison de la complexité logique de ce mod, il peut encore contenir des bugs. Sauvegardez donc votre partie avant de l'utiliser !

# Diplomatic Marriage Plus (DMP)

Ce mod est également connu sous le nom de "Mariage diplomatique & Alliance permanente". Il s'inspire fortement du célèbre jeu de Paradox - Crusader Kings.

Vous pouvez désormais former une alliance permanente en mariant un enfant du leader de votre colonie à une personne importante de l'une de vos alliées, faisant ainsi de cette dernière votre belle-famille, vous apportant plus d'aide sans vous soucier de la relation avec cette faction.

Même si votre enfant n'appartient plus à votre colonie après le mariage, il continuera à travailler en votre nom au sein de votre allié en maintenant des relations diplomatiques, et ils reviendront occasionnellement dans votre colonie pour un certain temps.

# Caractéristiques :

- Après que votre idéologie ait un leader, si celui-ci a un enfant adulte célibataire, une faction alliée avec une relation de plus de 75 vous proposera aléatoirement un mariage.
- Une fois le mariage conclu, l'enfant vous quittera pour rejoindre l'allié. Par la suite, en fonction de sa compétence sociale, la relation avec votre allié augmentera chaque jour (l'algorithme peut être ajusté dans la config du mod). Tant que le mariage reste actif, la relation peut facilement rester à 100 presque en permanence. La décroissance naturelle peut être rapidement compensée.
- Vous pouvez profiter des divers avantages fournis par votre allié permanent, tels que l'appel de caravanes et de renforts militaires, sans vous soucier de nuir trop de relation.
- Votre coloniste propagera votre idéologie au sein de votre allié, pouvant même finir par convertir toute la faction.
- Après l'alliance permanente, il y aura quelques événements aléatoires ultérieurs. La caravane alliée avec le couple marié parmi eux peut venir chercher refuge dans votre colonie de manière aléatoire. Pour maintenir l'alliance, vous devez protéger le couple à tout prix contre l'invasion à venir contre la caravane. (À noter : il ne sera parfois pas facile de protéger un coloniste qui n'est pas sous votre contrôle !)
- Votre enfant ressentira parfois la nostalgie et reviendra dans votre colonie en tant que coloniste temporaire avec son conjoint ou sa conjointe. Considérez le comme une opportunité pour les entraîner, leur donner des armes, armures ou implants, afin qu'ils aient moins de chances de mourir dans les batailles futures après leur retour chez l'allié. Avec le DLC Biotech, vous pouvez même les rendre immortels afin que l'alliance permanente puisse durer éternellement.

# COMPATIBILITÉ

Ce mod ne modifie aucun contenu vanilla, mais ajoute simplement quelques nouveaux incidents aléatoires gérés par mon propre script, il est donc très peu probable qu'il entre en conflit avec quoi que ce soit.
Cependant, si vous utilisez certains mods en même temps, des effets inattendus peuvent se produire dans certains cas. Par exemple :
- Certains des nouveaux mèmes fournis par Vanilla Ideology Expanded changeront votre leader d'idéologie par la force, en fonction de leurs compétences ou de leurs titres de royauté, ce qui peut mettre fin de manière inattendue à l'alliance permanente.
- Dynamic Diplomacy changera occasionnellement l'idéologie d'une faction IA, ce qui peut également annuler vos efforts pour convertir votre allié permanent (vous pouvez le désactiver dans les configs).
Vous devez toujours faire attention lorsque vous utilisez des mods comme ceux-ci, en particulier pour qu'ils ne contredisent les conditions de votre alliance permanente (voir FAQ). Mais pour un mod qui modifie une faction IA et la diplomacie, ce genre de problèmes existe toujours.

# Prérequis du DLC

    Idéologie


# FAQ :

- N'y a-t-il pas déjà une fonctionnalité similaire dans More Faction Interactions (MFI) ? Pourquoi avez-vous besoin de créer un autre mod dédié ?
MFI est un mod que je joue depuis longtemps. Je l'adore beaucoup personnellement. Mon mod est également inspiré par MFI, c'est pourquoi je l'ai appelé Diplomatic Marriage PLUS. Même beaucoup de mes codes sources sont appris via MFI.
Cependant, il y a trop d'incidents divers dans MFI, et il est impossible de creuser trop en profondeur sur un seul incident d'entre eux. L'événement de mariage diplomatique dans MFI ne vous permet que de sacrifier un coloniste au hasard en échange d'un petit bonus de relation ponctuel, qui, d'après moi, ne vaut pas vraiment la peine.
Ce mod, en revanche, est inspiré par le jeu Paradox Crusader Kings, qui étend considérablement l'événement de mariage diplomatique, lui donnant un véritable objectif de former une union permanente avec une faction IA, les mettant à votre service à long terme. En un mot, il vous apportera des avantages à long terme plutôt qu'un petit bonus ponctuel.
Plus encore, votre enfant qui vous quitte pour le mariage ne sera pas perdu pour toujours comme l'événement similaire dans MFI. Ils interagiront toujours fréquemment avec vous, vous faisant vraiment sentir qu'ils vous appartiennent toujours.

- Si j'utilise MFI en même temps, est-ce que deux événements de mariage diplomatique similaires entreront en conflit ?
Non, je suis uniquement inspiré par MFI, mais ce mod est 100% autonome. Si vous avez MFI, les deux événements de mariage diplomatique peuvent se déclencher indépendamment.

- Dans quelles circonstances une alliance permanente expirera-t-elle ?
1. Décès de l'un ou des deux colonistes impliqués dans le mariage
2. La faction alliée est détruite (Impossible dans le jeu de base mais cela peut se produire avec des mods tels que Dynamic Diplomacy ou Rim War)
3. Changement de leader de votre idéologie. Pour maintenir l'alliance, le leader de votre idéologie doit avoir un lien de parenté avec le coloniste marié. Remplacer par une personne non apparentée en tant que nouveau leader invalidera immédiatement l'alliance permanente. Mais si le leadership est simplement transféré du père à la mère, ou vice versa, l'alliance ne sera pas rompue.
4. La relation est trop faible. Même si elle augmente rapidement chaque jour, tomber en dessous de 0 peut toujours entraîner la rupture de l'alliance permanente. Vous devez donc faire particulièrement attention aux tirs amis lorsque votre allié est présent, surtout si vous êtes un fan d'unités mécaniques comme Diabolus ou Centipède.

- Que se passe-t-il après l'expiration de l'alliance permanente ?
En fonction de la raison, l'enfant de votre leader reviendra seul, mourra ou restera dans l'ex-allié permanent pour toujours. De plus, si la relation est assez élevée lorsque l'alliance permanente expire, ils resteront quand même en tant qu'allié classique.

- Après l'expiration de l'alliance permanente, puis-je former une autre à nouveau ?
Oui, tant que les conditions sont remplies, il n'y a pas de limite au nombre total d'alliances permanentes que vous pouvez avoir à chaque partie, que ce soit avec la même faction ou une différente. Mais une seule alliance permanente est autorisée à la fois.

- Pourquoi ma relation avec l'alliance permanente continue-t-elle de diminuer ?
L'alliance permanente n'éliminera pas la décroissance naturelle de la relation, ni le coût de la relation pour appeler des renforts ou des caravanes, etc. Elle augmentera simplement la relation chaque jour pour les compenser.

- Compatible avec les sauvegardes ?
Oui. Mais comme la plupart des mods, je ne peux pas garantir la sécurité de sa suppression à partir de sauvegardes existantes.


# Langues supportées :

À présent, j'ai fait des versions dans trois langues (qui sont également les langues que je parle IRL).
    - Chinois simplifié
    - Anglais
    - Français
J'apprécie si qqn peut le traduire dans d'autres langues, à condition de savoir comment le faire avec ces fichiers XML. Mais comme ce mod est encore incomplet, le texte peut encore changer à l'avenir.


# Problèmes connus actuellement :

- Avec seulement le DLC Ideology, il arrive parfois que les ennemis mécaniques ne puissent pas être générés. Mais je n'ai pas rencontré un tel problème avec les DLC Biotech et Royalty actifs.
- Je ne sais pas ce qui se passera si vous recommencez avec Archonexus, car je n'ai aucune idée de comment fonctionne cette quête.


# Remerciements

C'est mon premier mod de Rimworld. Un grand merci aux auteurs des mods suivants :

- More Faction Interactions : Bien que l'événement de mariage diplomatique soit simple, c'est là que j'ai trouvé l'inspiration pour créer ce mod. En même temps, le code source sur github m'a aussi beaucoup aidé.
- Factional War : Merci à l'auteur pour son code source rendue publique et ses commentaires clairs, grâce auxquels j'ai appris comment générer des armées NPC et les ordonner de se battre entre elles, car ce mod implique également cette fonctionnalité.
- Dynamic Diplomacy : Merci également à l'auteur d'avoir publié le code source. J'ai beaucoup appris sur la gestion des incidents.

Ce sont des mods auxquels j'ai joué tout le temps. Maintenant, je les aime encore plus après avoir appris de leur code source. Fortement recommandés.

Je tiens également à remercier ceux qui m'ont aidé dans le canal de développement de mod Discord du serveur Rimworld, ils ont répondu à certaines de mes questions techniques avec une rapidité incroyable. (Je suis désolé car je ne me souviens pas de vos ID car je n'ai pas posé beaucoup de questions).


# Autres mods recommandés :

- Not My Fault : Ce mod empêche les factions NPC de réduire la relation pour certaines causes déraisonnables, en particulier celles qui ne sont pas de votre faute (comme une caravane alliée affrontant un cluster mécanique sur la carte de votre colonie). Cela aide à réduire la probabilité de rompre injustement votre alliance permanente.
- Better Traders : Permet aux commerçants NPC d'avoir plus de marchandises et d'argent à échanger, facilitant l'approvisionnement par votre allié permanent.


# Plan de suivi :

Ce mod est loin d'être terminé. La version actuelle n'est qu'en bêta. J'ai encore beaucoup d'inspirations qui n'ont pas encore été implémentées.

Par exemple :
- Si le couple meurt au combat, permettre une période de grâce pour les ressusciter avant de rompre l'alliance permanente.
- Introduire en outre des éléments de Crusader King comme la Guerre de Succession, la répression des revendications, comme aider votre beau-fils/belle-fille à devenir le leader de votre allié et consolider davantage l'alliance permanente pour plus d'avantages.
- Ajouter plus d'interactions avec l'allié permanent, y compris aider les alliés à conquérir les ennemis et à reigner la planète entière (similaire au mod Empire, mais vous n'avez pas besoin de gérer autant de colonies, mais laissez votre allié le faire pour vous).
- Plus d'incidents où les deux mariés sont en danger et cherchent de l'aide, rendant plus difficile le maintien de l'alliance permanente. (Par exemple, certaines quêtes pour sauver deux colonistes, et les faire mourir si la quête échoue, etc.)
- Fournir un moyen de choisir activement une faction pour le mariage (comme une station de radio, ou une caravane de joueur visitant le village NPC, etc.)
- Avec le DLC Biotech, le couple peut-il devenir enceinte et avoir des enfants ? Je ne suis pas sûr de la faisabilité technique pour les colonistes mondiaux des autres factions.
- Peut-être permettre d'obtenir une remise des commerçants de votre allié permanent ? (Je n'ai pas trouvé de moyen technique pour le faire)

Je ne sais peut-être pas comment mettre en œuvre certaines des idées ci-dessus techniquement, donc toute suggestion ou surtout des exemples de code sont les bienvenus.

Si quelqu'un veut m'aider à améliorer ce mod, vous pouvez également faire votre demande de fusion(pull requests) directement sur mon github.

Github
https://github.com/Ionfrigate12345/Rimworld_DiplomaticMarriagePlus

